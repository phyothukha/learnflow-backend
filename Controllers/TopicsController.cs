using Microsoft.AspNetCore.Mvc;
using learnflow_service.Dtos;
using learnflow_service.Services;

namespace learnflow_service.Controllers;

[ApiController]
[Route("v1/[controller]")]
public class TopicsController : ControllerBase
{
    private readonly ITopicService _topicService;

    public TopicsController(ITopicService topicService)
    {
        _topicService = topicService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<TopicResponse>>> Get([FromQuery] TopicQueryParameters query)
        => Ok(await _topicService.GetAllAsync(query));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TopicResponse>> Get(Guid id)
    {
        var topic = await _topicService.GetByIdAsync(id);
        return topic == null ? NotFound() : Ok(topic);
    }

    [HttpPost]
    public async Task<ActionResult<TopicResponse>> Post([FromBody] CreateTopicRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var created = await _topicService.CreateAsync(request);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<TopicResponse>> Patch(Guid id, [FromBody] UpdateTopicRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var updated = await _topicService.UpdateAsync(id, request);
        return updated == null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _topicService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
