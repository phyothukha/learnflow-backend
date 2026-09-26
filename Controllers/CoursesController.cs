using Microsoft.AspNetCore.Mvc;
using learnflow_service.Dtos;
using learnflow_service.Services;

namespace learnflow_service.Controllers;

[ApiController]
[Route("v1/[controller]")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<CourseResponse>>> Get([FromQuery] CourseQueryParameters query)
        => Ok(await _courseService.GetAllAsync(query));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CourseResponse>> Get(Guid id)
    {
        var course = await _courseService.GetByIdAsync(id);
        return course == null ? NotFound() : Ok(course);
    }

    [HttpPost]
    public async Task<ActionResult<CourseResponse>> Post([FromBody] CreateCourseRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var created = await _courseService.CreateAsync(request);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<CourseResponse>> Patch(Guid id, [FromBody] UpdateCourseRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var updated = await _courseService.UpdateAsync(id, request);
        return updated == null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _courseService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
