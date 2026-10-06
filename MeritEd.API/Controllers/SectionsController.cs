using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MeritEd.API.DTOs.Sections;
using MeritEd.Core.Interfaces.Services;

namespace MeritEd.API.Controllers;

[ApiController]
[Route("api/courses/{courseId}/sections")]
[Authorize]
public class SectionsController : ControllerBase
{
    private readonly ISectionService _sectionService;

    public SectionsController(ISectionService sectionService)
    {
        _sectionService = sectionService;
    }

    private Guid GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(sub!);
    }

    [HttpGet]
    public async Task<IActionResult> GetSections(Guid courseId)
    {
        var sections = await _sectionService.GetSectionsByCourseAsync(courseId);

        var response = sections.Select(s => new SectionResponse
        {
            Id = s.Id,
            CourseId = s.CourseId,
            Title = s.Title,
            OrderIndex = s.OrderIndex,
            IsPublished = s.IsPublished,
            CreatedAt = s.CreatedAt,
            ContentItemCount = s.ContentItems.Count
        });

        return Ok(response);
    }

    [HttpPost]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> CreateSection(Guid courseId, CreateSectionRequest request)
    {
        try
        {
            var instructorId = GetCurrentUserId();
            var section = await _sectionService.CreateSectionAsync(courseId, instructorId, request.Title);

            return CreatedAtAction(nameof(GetSections), new { courseId }, new SectionResponse
            {
                Id = section.Id,
                CourseId = section.CourseId,
                Title = section.Title,
                OrderIndex = section.OrderIndex,
                IsPublished = section.IsPublished,
                CreatedAt = section.CreatedAt,
                ContentItemCount = 0
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPut("{sectionId}")]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> UpdateSection(Guid courseId, Guid sectionId,
        UpdateSectionRequest request)
    {
        var instructorId = GetCurrentUserId();
        var section = await _sectionService.UpdateSectionAsync(
            sectionId, instructorId, request.Title, request.IsPublished);

        if (section == null)
            return NotFound(new { error = "Section not found or you do not own this course." });

        return Ok(new SectionResponse
        {
            Id = section.Id,
            CourseId = section.CourseId,
            Title = section.Title,
            OrderIndex = section.OrderIndex,
            IsPublished = section.IsPublished,
            CreatedAt = section.CreatedAt,
            ContentItemCount = section.ContentItems.Count
        });
    }

    [HttpDelete("{sectionId}")]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> DeleteSection(Guid courseId, Guid sectionId)
    {
        var instructorId = GetCurrentUserId();
        var result = await _sectionService.DeleteSectionAsync(sectionId, instructorId);

        if (!result)
            return NotFound(new { error = "Section not found or you do not own this course." });

        return Ok(new { message = "Section deleted successfully." });
    }

    [HttpPut("reorder")]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> ReorderSections(Guid courseId, ReorderSectionsRequest request)
    {
        var instructorId = GetCurrentUserId();
        var result = await _sectionService.ReorderSectionsAsync(courseId, instructorId, request.OrderedIds);

        if (!result)
            return NotFound(new { error = "Course not found or you do not own this course." });

        return Ok(new { message = "Sections reordered successfully." });
    }
}