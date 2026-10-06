using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using MeritEd.API.DTOs.ContentItems;
using MeritEd.Core.Entities;
using MeritEd.Core.Interfaces.Services;

namespace MeritEd.API.Controllers;

[ApiController]
[Route("api/sections/{sectionId}/items")]
[Authorize]
public class ContentItemsController : ControllerBase
{
    private readonly IContentItemService _contentItemService;

    public ContentItemsController(IContentItemService contentItemService)
    {
        _contentItemService = contentItemService;
    }

    private Guid GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(sub!);
    }

    private static ContentItemResponse MapToResponse(ContentItem item)
    {
        var response = new ContentItemResponse
        {
            Id = item.Id,
            SectionId = item.SectionId,
            Type = item.GetType().Name,
            Title = item.Title,
            OrderIndex = item.OrderIndex,
            IsPublished = item.IsPublished,
            CreatedAt = item.CreatedAt
        };

        switch (item)
        {
            case Lecture lecture:
                response.Body = lecture.Body;
                break;
            case FileItem file:
                response.FileUrl = file.FileUrl;
                response.FileSize = file.FileSize;
                response.MimeType = file.MimeType;
                break;
            case LinkItem link:
                response.ExternalUrl = link.ExternalUrl;
                response.Description = link.Description;
                break;
            case Assignment assignment:
                response.Description = assignment.Description;
                response.DueDate = assignment.DueDate;
                response.BaseXP = assignment.BaseXP;
                response.AllowsRecovery = assignment.AllowsRecovery;
                break;
            case Quiz quiz:
                response.DueDate = quiz.DueDate;
                response.BaseXP = quiz.BaseXP;
                response.AllowsRecovery = quiz.AllowsRecovery;
                response.Questions = quiz.Questions?.RootElement.ToString();
                break;
        }

        return response;
    }

    [HttpGet]
    public async Task<IActionResult> GetContentItems(Guid sectionId)
    {
        var isInstructor = User.IsInRole("Instructor");
        var items = await _contentItemService.GetContentItemsBySectionAsync(sectionId, isInstructor);
        return Ok(items.Select(MapToResponse));
    }

    [HttpPost]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> CreateContentItem(Guid sectionId,
        CreateContentItemRequest request)
    {
        try
        {
            var instructorId = GetCurrentUserId();
            var item = await _contentItemService.CreateContentItemAsync(
                sectionId, instructorId, request.Type, request.Title,
                request.Body, request.FileUrl, request.FileSize, request.MimeType,
                request.ExternalUrl, request.Description, request.DueDate,
                request.BaseXP, request.AllowsRecovery, request.Questions);

            return CreatedAtAction(nameof(GetContentItems),
                new { sectionId }, MapToResponse(item));
        }
        catch (UnauthorizedAccessException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPut("{itemId}/publish")]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> UpdatePublishStatus(Guid sectionId, Guid itemId,
        UpdatePublishStatusRequest request)
    {
        var instructorId = GetCurrentUserId();
        var item = await _contentItemService.UpdatePublishStatusAsync(
            itemId, instructorId, request.IsPublished);

        if (item == null)
            return NotFound(new { error = "Content item not found or you do not own this course." });

        return Ok(MapToResponse(item));
    }

    [HttpDelete("{itemId}")]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> DeleteContentItem(Guid sectionId, Guid itemId)
    {
        var instructorId = GetCurrentUserId();
        var result = await _contentItemService.DeleteContentItemAsync(itemId, instructorId);

        if (!result)
            return NotFound(new { error = "Content item not found or you do not own this course." });

        return Ok(new { message = "Content item deleted successfully." });
    }

    [HttpPut("reorder")]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> ReorderContentItems(Guid sectionId,
        ReorderContentItemsRequest request)
    {
        var instructorId = GetCurrentUserId();
        var result = await _contentItemService.ReorderContentItemsAsync(
            sectionId, instructorId, request.OrderedIds);

        if (!result)
            return NotFound(new { error = "Section not found or you do not own this course." });

        return Ok(new { message = "Content items reordered successfully." });
    }
}