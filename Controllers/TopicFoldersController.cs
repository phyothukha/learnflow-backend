using Microsoft.AspNetCore.Mvc;
using learnflow_service.Dtos;
using learnflow_service.Services;

namespace learnflow_service.Controllers;

[ApiController]
[Route("v1/[controller]")]
public class TopicFoldersController : ControllerBase
{
    private readonly ITopicFolderService _folderService;

    public TopicFoldersController(ITopicFolderService folderService)
    {
        _folderService = folderService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<TopicFolderResponse>>> Get([FromQuery] TopicFolderQueryParameters query)
        => Ok(await _folderService.GetAllAsync(query));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TopicFolderResponse>> Get(Guid id)
    {
        var folder = await _folderService.GetByIdAsync(id);
        return folder == null ? NotFound() : Ok(folder);
    }

    [HttpGet("tree")]
    public async Task<ActionResult<List<TopicFolderTreeNode>>> GetTree([FromQuery] Guid topicId)
        => Ok(await _folderService.GetTreeAsync(topicId));

    [HttpPost]
    public async Task<ActionResult<TopicFolderResponse>> Post([FromBody] CreateTopicFolderRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var created = await _folderService.CreateAsync(request);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<TopicFolderResponse>> Patch(Guid id, [FromBody] UpdateTopicFolderRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var updated = await _folderService.UpdateAsync(id, request);
        return updated == null ? NotFound() : Ok(updated);
    }

    [HttpPost("{id:guid}/move")]
    public async Task<ActionResult<TopicFolderResponse>> Move(Guid id, [FromBody] MoveTopicFolderRequest request)
    {
        var (result, folder) = await _folderService.MoveAsync(id, request);

        return result switch
        {
            MoveFolderResult.Success => Ok(folder),
            MoveFolderResult.NotFound => NotFound(),
            MoveFolderResult.InvalidParent => BadRequest(new { message = "Target parent folder does not exist in this topic." }),
            MoveFolderResult.WouldCreateCycle => BadRequest(new { message = "Cannot move a folder into its own descendant." }),
            _ => BadRequest()
        };
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _folderService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
