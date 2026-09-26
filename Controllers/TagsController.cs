using Microsoft.AspNetCore.Mvc;
using learnflow_service.Dtos;
using learnflow_service.Services;

namespace learnflow_service.Controllers;

[ApiController]
[Route("v1/[controller]")]
public class TagsController : ControllerBase
{
    private readonly ITagService _tagService;

    public TagsController(ITagService tagService)
    {
        _tagService = tagService;
    }

    [HttpGet]
    public async Task<ActionResult<List<TagResponse>>> Get()
        => Ok(await _tagService.GetAllAsync());
}
