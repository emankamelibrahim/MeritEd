using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MeritEd.Core.Entities;

namespace MeritEd.API.Data;

public class AppDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Section> Sections => Set<Section>();
    public DbSet<ContentItem> ContentItems => Set<ContentItem>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<XPTransaction> XPTransactions => Set<XPTransaction>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ContentItem>()
            .HasDiscriminator<string>("Type")
            .HasValue<Lecture>("Lecture")
            .HasValue<FileItem>("File")
            .HasValue<LinkItem>("Link")
            .HasValue<Assignment>("Assignment")
            .HasValue<Quiz>("Quiz");

        // Resolve column name conflicts across TPH subtypes
        builder.Entity<LinkItem>()
            .Property(l => l.Description)
            .HasColumnName("LinkDescription");

        builder.Entity<Quiz>()
            .Property(q => q.DueDate)
            .HasColumnName("QuizDueDate");

        builder.Entity<Quiz>()
            .Property(q => q.BaseXP)
            .HasColumnName("QuizBaseXP");

        builder.Entity<Quiz>()
            .Property(q => q.AllowsRecovery)
            .HasColumnName("QuizAllowsRecovery");

        builder.Entity<Enrollment>()
            .HasIndex(e => new { e.StudentId, e.CourseId })
            .IsUnique();
    }
}