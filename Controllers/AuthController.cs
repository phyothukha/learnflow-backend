using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using learnflow_service.Models;

namespace learnflow_service.Controllers;

[ApiController]
[Route("v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _readDb;
    private readonly ILogger<AuthController> _logger;
    private readonly JwtSettings _jwt;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string? _firebaseKey = Environment.GetEnvironmentVariable("FIREBASE_API_KEY");

    private static bool FirebaseEnabled => FirebaseAdmin.FirebaseApp.DefaultInstance != null;

    public AuthController(
        [FromKeyedServices("read")] ApplicationDbContext readDb,
        ILogger<AuthController> logger,
        JwtSettings jwt,
        IHttpClientFactory httpClientFactory)
    {
        _readDb = readDb;
        _logger = logger;
        _jwt = jwt;
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "Email and password are required." });

        // Step 1: Check AdminUsers table
        var user = await _readDb.AdminUsers
            .Where(u => u.Email == request.Email && u.IsActive == true)
            .Select(u => new
            {
                u.Id,
                u.Email,
                u.Name,
                u.PasswordHash,
                Roles = u.UserRoles
                    .Select(ur => ur.Role.Name)
                    .ToList(),
                // Project raw columns only — string concat + Distinct inside a
                // correlated collection projection is not translatable by EF Core.
                PermissionPairs = u.UserRoles
                    .SelectMany(ur => ur.Role.RolePermissions)
                    .Select(rp => new { rp.Permission.Resource, rp.Permission.Action })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (user == null)
        {
            _logger.LogWarning("Login failed - user not found or inactive: {Email}", request.Email);
            return Unauthorized(new { message = "Invalid email or password." });
        }

        // Step 2: Verify password via BCrypt
        if (string.IsNullOrWhiteSpace(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed - invalid password for user: {Email}", request.Email);
            return Unauthorized(new { message = "Invalid email or password." });
        }

        // Step 3: Sync with Firebase when configured — create if not exists, sign in if exists
        FirebaseTokens? firebaseTokens = null;
        if (FirebaseEnabled)
        {
            var firebaseResult = await VerifyOrCreateFirebaseUserAsync(request.Email, request.Password);
            if (!firebaseResult.Success)
            {
                _logger.LogWarning("Login failed - Firebase sync error: {Email}", request.Email);
                return Unauthorized(new { message = "Invalid email or password." });
            }

            if (firebaseResult.WasCreated)
                _logger.LogInformation("Firebase user auto-created for existing DB user: {Email}", request.Email);

            firebaseTokens = await GetFirebaseTokensAsync(user.Email, request.Password);
        }

        // Step 4: Generate JWT
        var permissions = user.PermissionPairs
            .Select(p => p.Resource + "_" + p.Action)
            .Distinct()
            .ToList();
        var token = GenerateJwtToken(user.Id, user.Email, user.Roles);

        _logger.LogInformation("User {Email} logged in successfully", user.Email);

        return Ok(new
        {
            token,
            firebaseToken = firebaseTokens?.IdToken,
            firebaseRefreshToken = firebaseTokens?.RefreshToken,
            user = new
            {
                user.Id,
                user.Email,
                user.Name,
                user.Roles,
                Permissions = permissions
            }
        });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return BadRequest(new { message = "Refresh token is required." });

        if (string.IsNullOrEmpty(_firebaseKey))
            return StatusCode(StatusCodes.Status501NotImplemented, new { message = "Firebase is not configured." });

        var client = _httpClientFactory.CreateClient();
        var response = await client.PostAsJsonAsync(
            $"https://securetoken.googleapis.com/v1/token?key={_firebaseKey}",
            new { grant_type = "refresh_token", refresh_token = request.RefreshToken });

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Firebase token refresh failed: {Status}", response.StatusCode);
            return Unauthorized(new { message = "Invalid or expired refresh token." });
        }

        using var doc = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var idToken = doc.RootElement.GetProperty("id_token").GetString();
        var newRefreshToken = doc.RootElement.GetProperty("refresh_token").GetString();

        return Ok(new
        {
            firebaseToken = idToken,
            firebaseRefreshToken = newRefreshToken
        });
    }

    private async Task<FirebaseTokens?> GetFirebaseTokensAsync(string email, string password)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync(
                $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={_firebaseKey}",
                new { email, password, returnSecureToken = true });

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Firebase signInWithPassword failed: {Status}", response.StatusCode);
                return null;
            }

            using var doc = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return new FirebaseTokens(
                doc.RootElement.GetProperty("idToken").GetString()!,
                doc.RootElement.GetProperty("refreshToken").GetString()!
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Firebase tokens for {Email}", email);
            return null;
        }
    }

    private record FirebaseTokens(string IdToken, string RefreshToken);

    private async Task<FirebaseAuthResult> VerifyOrCreateFirebaseUserAsync(string email, string password)
    {
        // BCrypt already verified the password — use Admin SDK to manage Firebase user directly
        try
        {
            FirebaseAdmin.Auth.UserRecord? firebaseUser = null;
            try
            {
                firebaseUser = await FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance
                    .GetUserByEmailAsync(email);
            }
            catch (FirebaseAdmin.Auth.FirebaseAuthException ex)
                when (ex.AuthErrorCode == FirebaseAdmin.Auth.AuthErrorCode.UserNotFound)
            {
                // User doesn't exist in Firebase yet — will create below
            }

            if (firebaseUser == null)
            {
                await FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance.CreateUserAsync(
                    new FirebaseAdmin.Auth.UserRecordArgs
                    {
                        Email = email,
                        Password = password,
                        EmailVerified = true,
                        Disabled = false
                    });

                _logger.LogInformation("Firebase user created for {Email}", email);
                return new FirebaseAuthResult(true, true);
            }

            // User exists — sync password to keep Firebase in step with DB
            await FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance.UpdateUserAsync(
                new FirebaseAdmin.Auth.UserRecordArgs
                {
                    Uid = firebaseUser.Uid,
                    Password = password
                });

            return new FirebaseAuthResult(true, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Firebase sync failed for {Email}", email);
            return new FirebaseAuthResult(false, false);
        }
    }

    private record FirebaseAuthResult(bool Success, bool WasCreated);

    private string GenerateJwtToken(Guid userId, string email, IList<string> roles)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(_jwt.ExpiryHours),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RefreshRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}
