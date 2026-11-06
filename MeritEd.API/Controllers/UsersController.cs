using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MeritEd.API.Data;
using MeritEd.API.DTOs.Users;
using MeritEd.Core.Enums;

namespace MeritEd.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        var users = await _context.Users
            .Where(u => u.Role == UserRole.Instructor && !u.IsApproved)
            .OrderBy(u => u.CreatedAt)
            .Select(u => new PendingInstructorResponse
            {
                Id = u.Id,
                Email = u.Email!,
                DisplayName = u.DisplayName,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpPut("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.Role == UserRole.Instructor);

        if (user == null)
            return NotFound(new { error = "Instructor not found." });

        user.IsApproved = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Instructor approved." });
    }
}