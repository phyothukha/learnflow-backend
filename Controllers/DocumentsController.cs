using Microsoft.AspNetCore.Mvc;
using learnflow_service.Dtos;
using learnflow_service.Services;

namespace learnflow_service.Controllers;

[ApiController]
[Route("v1/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public DocumentsController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<DocumentResponse>>> Get([FromQuery] DocumentQueryParameters query)
        => Ok(await _documentService.GetAllAsync(query));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DocumentResponse>> Get(Guid id)
    {
        var document = await _documentService.GetByIdAsync(id);
        return document == null ? NotFound() : Ok(document);
    }

    [HttpPost]
    public async Task<ActionResult<DocumentResponse>> Post([FromBody] CreateDocumentRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var created = await _documentService.CreateAsync(request);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<DocumentResponse>> Patch(Guid id, [FromBody] UpdateDocumentRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var updated = await _documentService.UpdateAsync(id, request);
        return updated == null ? NotFound() : Ok(updated);
    }

    [HttpPost("{id:guid}/move")]
    public async Task<ActionResult<DocumentResponse>> Move(Guid id, [FromBody] MoveDocumentRequest request)
    {
        var moved = await _documentService.MoveAsync(id, request);
        return moved == null ? NotFound() : Ok(moved);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _documentService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/attachments")]
    [RequestSizeLimit(25_000_000)]
    public async Task<ActionResult<AttachmentResponse>> UploadAttachment(Guid id, IFormFile file)
    {
        if (file.Length == 0) return BadRequest(new { message = "File is empty." });

        await using var stream = file.OpenReadStream();
        var result = await _documentService.AddAttachmentAsync(id, stream, file.FileName, file.ContentType, file.Length);

        return result.Status switch
        {
            AttachmentUploadStatus.Success => Ok(result.Attachment),
            AttachmentUploadStatus.DocumentNotFound => NotFound(),
            AttachmentUploadStatus.StorageNotConfigured =>
                StatusCode(StatusCodes.Status501NotImplemented, new { message = "File storage is not configured." }),
            _ => BadRequest()
        };
    }

    [HttpDelete("{id:guid}/attachments/{attachmentId:guid}")]
    public async Task<IActionResult> DeleteAttachment(Guid id, Guid attachmentId)
    {
        var deleted = await _documentService.DeleteAttachmentAsync(id, attachmentId);
        return deleted ? NoContent() : NotFound();
    }
}
