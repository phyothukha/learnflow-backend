using System.Text;
using System.Text.Json.Serialization;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OData;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using learnflow_service.Mapping;
using learnflow_service.Models;
using learnflow_service.Utils;
using Serilog;
using Serilog.Sinks.Grafana.Loki;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestLineSize = 1024 * 64;
    options.Limits.MaxRequestHeadersTotalSize = 1024 * 64;
});

DotNetEnv.Env.Load();

Serilog.Debugging.SelfLog.Enable(msg => Console.WriteLine($"[SERILOG] {msg}"));

var lokiUrl = Environment.GetEnvironmentVariable("LOKI_URL");
var lokiUser = Environment.GetEnvironmentVariable("LOKI_USER");
var lokiApiKey = Environment.GetEnvironmentVariable("LOKI_API_KEY");
var environment = Environment.GetEnvironmentVariable("ENVIRONMENT");

if (string.IsNullOrEmpty(lokiUser) || string.IsNullOrEmpty(lokiApiKey))
{
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .CreateLogger();

    Log.Warning("Loki credentials not found. Logs will only be sent to console.");
}
else
{
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .Enrich.FromLogContext()
        .Enrich.WithProperty("MachineName", Environment.MachineName)
        .WriteTo.Console()
        .WriteTo.GrafanaLoki(
            uri: lokiUrl!,
            labels: new[]
            {
                new LokiLabel { Key = "api", Value = "learnflow-service" },
                new LokiLabel { Key = "environment", Value = environment ?? "unknown" },
                new LokiLabel { Key = "machine", Value = Environment.MachineName },
            },
            credentials: new LokiCredentials { Login = lokiUser, Password = lokiApiKey },
            period: TimeSpan.FromSeconds(2),
            batchPostingLimit: 100
        )
        .CreateLogger();

    Log.Information("✓ Serilog configured with Grafana Loki. URL: {LokiUrl}, User: {LokiUser}", lokiUrl, lokiUser);
}

builder.Host.UseSerilog();

try
{
    Log.Information("Starting up learnflow service");

    string? writeConnectionString = Environment.GetEnvironmentVariable("WRITE_CONNECTION_STRING");
    string? readConnectionString = Environment.GetEnvironmentVariable("READ_CONNECTION_STRING");

    if (string.IsNullOrEmpty(writeConnectionString))
        throw new InvalidOperationException("WRITE_CONNECTION_STRING environment variable is not set.");

    if (string.IsNullOrEmpty(readConnectionString))
        throw new InvalidOperationException("READ_CONNECTION_STRING environment variable is not set.");

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(writeConnectionString));

    builder.Services.AddKeyedScoped<ApplicationDbContext>(
        "write",
        (sp, key) =>
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql(writeConnectionString);
            return new ApplicationDbContext(optionsBuilder.Options);
        });

    builder.Services.AddKeyedScoped<ApplicationDbContext>(
        "read",
        (sp, key) =>
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql(readConnectionString);
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            return new ApplicationDbContext(optionsBuilder.Options);
        });

    builder.Services
        .AddControllers()
        .AddOData(opt =>
        {
            // Treat all DateTime payloads as UTC — otherwise OData converts
            // incoming/outgoing values through the server's local timezone.
            opt.TimeZone = TimeZoneInfo.Utc;
            opt.AddRouteComponents(
                    "v1",
                    GetEdmModel(),
                    services =>
                        services.AddSingleton<
                            Microsoft.AspNetCore.OData.Batch.ODataBatchHandler,
                            Microsoft.AspNetCore.OData.Batch.DefaultODataBatchHandler>())
                .Filter()
                .OrderBy()
                .Count()
                .Select()
                .Expand()
                .SetMaxTop(100);
        })
        .AddJsonOptions(options =>
        {
            // PascalCase like the OData endpoints (frontend interfaces expect it).
            options.JsonSerializerOptions.PropertyNamingPolicy = null;
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            options.JsonSerializerOptions.Converters.Add(new FlexibleTimeSpanJsonConverter());
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

    TypeAdapterConfig.GlobalSettings.Scan(typeof(MappingConfig).Assembly);
    builder.Services.AddMapster();

    builder.Services.AddScoped<learnflow_service.Repositories.ICourseRepository, learnflow_service.Repositories.CourseRepository>();
    builder.Services.AddScoped<learnflow_service.Services.ICourseService, learnflow_service.Services.CourseService>();

    builder.Services.AddScoped<learnflow_service.Repositories.ITopicRepository, learnflow_service.Repositories.TopicRepository>();
    builder.Services.AddScoped<learnflow_service.Services.ITopicService, learnflow_service.Services.TopicService>();

    builder.Services.AddScoped<learnflow_service.Repositories.ITopicFolderRepository, learnflow_service.Repositories.TopicFolderRepository>();
    builder.Services.AddScoped<learnflow_service.Services.ITopicFolderService, learnflow_service.Services.TopicFolderService>();

    builder.Services.AddScoped<learnflow_service.Repositories.INoteRepository, learnflow_service.Repositories.NoteRepository>();
    builder.Services.AddScoped<learnflow_service.Services.INoteService, learnflow_service.Services.NoteService>();

    builder.Services.AddScoped<learnflow_service.Repositories.ITagRepository, learnflow_service.Repositories.TagRepository>();
    builder.Services.AddScoped<learnflow_service.Services.ITagService, learnflow_service.Services.TagService>();

    builder.Services.AddScoped<learnflow_service.Repositories.IDocumentRepository, learnflow_service.Repositories.DocumentRepository>();
    builder.Services.AddScoped<learnflow_service.Services.IDocumentService, learnflow_service.Services.DocumentService>();

    // Firebase is optional for local development — the service boots without a credential file.
    var firebaseCredentialPath = Environment.GetEnvironmentVariable("FIREBASE_CREDENTIAL_PATH") ?? "prod_firebase.json";
    var firebaseStorageBucket = Environment.GetEnvironmentVariable("FIREBASE_STORAGE_BUCKET");
    Google.Cloud.Storage.V1.StorageClient? storageClient = null;
    if (File.Exists(firebaseCredentialPath))
    {
#pragma warning disable CS0618
        var credential = GoogleCredential.FromFile(firebaseCredentialPath);
        FirebaseApp.Create(new AppOptions
        {
            Credential = credential
        });
#pragma warning restore CS0618
        Log.Information("✓ Firebase Admin SDK initialized.");

        if (!string.IsNullOrEmpty(firebaseStorageBucket))
        {
            storageClient = Google.Cloud.Storage.V1.StorageClient.Create(credential);
            Log.Information("✓ Firebase Storage configured for bucket {Bucket}.", firebaseStorageBucket);
        }
        else
        {
            Log.Warning("FIREBASE_STORAGE_BUCKET not set. Attachment uploads disabled.");
        }
    }
    else
    {
        Log.Warning("Firebase credential file not found at {Path}. Firebase sync and storage disabled.", firebaseCredentialPath);
    }

    builder.Services.AddSingleton<learnflow_service.Services.IFileStorageService>(
        new learnflow_service.Services.FirebaseStorageService(storageClient, firebaseStorageBucket));

    var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET")
        ?? throw new InvalidOperationException("JWT_SECRET environment variable is not set.");
    var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "learnflow-service";
    var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "learnflow-clients";
    var jwtExpiryHours = int.TryParse(Environment.GetEnvironmentVariable("JWT_EXPIRY_HOURS"), out var h) ? h : 8;

    builder.Services.AddSingleton(new JwtSettings(jwtSecret, jwtIssuer, jwtAudience, jwtExpiryHours));

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,
                ValidateAudience = true,
                ValidAudience = jwtAudience,
                ClockSkew = TimeSpan.Zero
            };
        });

    builder.Services.AddHttpClient();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
        c.DocInclusionPredicate((_, apiDesc) =>
            apiDesc.RelativePath != null &&
            !apiDesc.RelativePath.Contains("$"));
    });

    var app = builder.Build();

    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

    var isLocal = environment == "dev";

    if (!isLocal)
        app.UsePathBase("/learn-flow");

    var swaggerPrefix = isLocal ? "" : "/learn-flow";

    app.UseSwagger();
    app.UseSwaggerUI(c =>
        c.SwaggerEndpoint($"{swaggerPrefix}/swagger/v1/swagger.json", "LearnFlow API v1"));

    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    Log.Information("LearnFlow service started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "LearnFlow service terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

static IEdmModel GetEdmModel()
{
    var builder = new ODataConventionModelBuilder();
    builder.EntitySet<Lesson>("Lessons");
    builder.EntitySet<Enrollment>("Enrollments");
    builder.EntitySet<AdminUser>("AdminUsers");
    builder.EntitySet<AdminRole>("AdminRoles");
    builder.EntitySet<AdminPermission>("AdminPermissions");
    builder.EntitySet<AdminRolePermission>("AdminRolePermissions");
    builder.EntitySet<AdminUserRole>("AdminUserRoles");
    builder.EntitySet<StudyBlock>("StudyBlocks");
    return builder.GetEdmModel();
}

public record JwtSettings(string Secret, string Issuer, string Audience, int ExpiryHours);
