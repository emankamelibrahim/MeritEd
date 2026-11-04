using Microsoft.EntityFrameworkCore;
using MeritEd.API.Data;
using MeritEd.API.DTOs.Badges;
using MeritEd.API.DTOs.Dashboard;
using MeritEd.API.DTOs.Progress;
using MeritEd.Core.Enums;
using MeritEd.Core.Services;

namespace MeritEd.API.Services;

public interface IDashboardService
{
    Task<StudentDashboardResponse> GetStudentDashboardAsync(Guid studentId);
    Task<InstructorDashboardResponse> GetInstructorDashboardAsync(Guid instructorId);
    Task<CourseDashboardResponse?> GetCourseDashboardAsync(Guid courseId, Guid instructorId);
}

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;
    private readonly OutlierService _outlierService;

    public DashboardService(AppDbContext context, OutlierService outlierService)
    {
        _context = context;
        _outlierService = outlierService;
    }

    private static List<XPBreakdownItem> BuildBreakdown(
        IEnumerable<(XPCategory Category, int Amount)> transactions, int total)
    {
        return transactions
            .GroupBy(t => t.Category)
            .Select(g => new XPBreakdownItem
            {
                Category = g.Key.ToString(),
                TotalXP = g.Sum(t => t.Amount),
                Percentage = total > 0
                    ? Math.Round((double)g.Sum(t => t.Amount) / total * 100, 1)
                    : 0
            })
            .ToList();
    }

    public async Task<StudentDashboardResponse> GetStudentDashboardAsync(Guid studentId)
    {
        var enrollments = await _context.Enrollments
            .Include(e => e.Course)
            .Where(e => e.StudentId == studentId && e.Status == EnrollmentStatus.Active)
            .ToListAsync();

        var ids = enrollments.Select(e => e.Id).ToList();

        var txRows = await _context.XPTransactions
            .Where(t => ids.Contains(t.EnrollmentId))
            .Select(t => new { t.EnrollmentId, t.Category, t.Amount })
            .ToListAsync();
        var txLookup = txRows.ToLookup(t => t.EnrollmentId);

        var badges = await _context.StudentBadges
            .Include(sb => sb.Badge)
            .Where(sb => ids.Contains(sb.EnrollmentId))
            .OrderBy(sb => sb.EarnedAt)
            .ToListAsync();
        var badgeLookup = badges.ToLookup(sb => sb.EnrollmentId);

        return new StudentDashboardResponse
        {
            Courses = enrollments.Select(e => new StudentCourseSummary
            {
                CourseId = e.CourseId,
                CourseTitle = e.Course.Title,
                TotalXP = e.TotalXP,
                IdentityType = e.IdentityType?.ToString(),
                XPBreakdown = BuildBreakdown(
                    txLookup[e.Id].Select(t => (t.Category, t.Amount)), e.TotalXP),
                Badges = badgeLookup[e.Id].Select(sb => new StudentBadgeResponse
                {
                    Id = sb.Id,
                    BadgeId = sb.BadgeId,
                    BadgeName = sb.Badge.Name,
                    BadgeDescription = sb.Badge.Description,
                    IconUrl = sb.Badge.IconUrl,
                    XPBonus = sb.Badge.XPBonus,
                    EarnedAt = sb.EarnedAt
                }).ToList()
            }).ToList()
        };
    }

    public async Task<InstructorDashboardResponse> GetInstructorDashboardAsync(Guid instructorId)
    {
        var courses = await _context.Courses
            .Where(c => c.InstructorId == instructorId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        var courseIds = courses.Select(c => c.Id).ToList();

        var enrollments = await _context.Enrollments
            .Where(e => courseIds.Contains(e.CourseId) && e.Status == EnrollmentStatus.Active)
            .Select(e => new { e.CourseId, e.TotalXP })
            .ToListAsync();
        var byCourse = enrollments.ToLookup(e => e.CourseId);

        return new InstructorDashboardResponse
        {
            Courses = courses.Select(c =>
            {
                var xps = byCourse[c.Id].Select(e => e.TotalXP).ToList();
                var median = _outlierService.CalculateMedian(xps);

                return new InstructorCourseSummary
                {
                    CourseId = c.Id,
                    Title = c.Title,
                    Status = c.Status.ToString(),
                    StudentCount = xps.Count,
                    MedianXP = median,
                    OutlierCount = xps.Count(xp =>
                        _outlierService.Classify(xp, median) != OutlierStatus.Normal)
                };
            }).ToList()
        };
    }

    public async Task<CourseDashboardResponse?> GetCourseDashboardAsync(
        Guid courseId, Guid instructorId)
    {
        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == courseId && c.InstructorId == instructorId);

        if (course == null) return null;

        var enrollments = await _context.Enrollments
            .Include(e => e.Student)
            .Where(e => e.CourseId == courseId && e.Status == EnrollmentStatus.Active)
            .ToListAsync();

        var ids = enrollments.Select(e => e.Id).ToList();

        var txRows = await _context.XPTransactions
            .Where(t => ids.Contains(t.EnrollmentId))
            .Select(t => new { t.EnrollmentId, t.Category, t.Amount })
            .ToListAsync();
        var txLookup = txRows.ToLookup(t => t.EnrollmentId);

        var xps = enrollments.Select(e => e.TotalXP).ToList();
        var median = _outlierService.CalculateMedian(xps);

        return new CourseDashboardResponse
        {
            CourseId = course.Id,
            CourseTitle = course.Title,
            StudentCount = enrollments.Count,
            MedianXP = median,
            AverageXP = xps.Count > 0 ? Math.Round(xps.Average(), 1) : 0,
            IdentityDistribution = enrollments
                .GroupBy(e => e.IdentityType?.ToString() ?? "None")
                .ToDictionary(g => g.Key, g => g.Count()),
            Students = enrollments
                .OrderByDescending(e => e.TotalXP)
                .Select(e => new StudentRow
                {
                    StudentId = e.StudentId,
                    StudentName = e.Student.DisplayName,
                    StudentEmail = e.Student.Email!,
                    TotalXP = e.TotalXP,
                    IdentityType = e.IdentityType?.ToString(),
                    Outlier = _outlierService.Classify(e.TotalXP, median).ToString(),
                    XPBreakdown = BuildBreakdown(
                        txLookup[e.Id].Select(t => (t.Category, t.Amount)), e.TotalXP)
                }).ToList()
        };
    }
}