using Microsoft.EntityFrameworkCore;
using EduConnect.Models;

namespace EduConnect.Data;

public class EduConnectDbContext : DbContext
{
    public EduConnectDbContext(DbContextOptions<EduConnectDbContext> options) : base(options) { }

    // ── DbSets ────────────────────────────────────────────────────────────
    public DbSet<Person> People { get; set; } = null!;         // TPH base table
    public DbSet<Student> Students { get; set; } = null!;      // Filtered view
    public DbSet<Faculty> FacultyMembers { get; set; } = null!; // Filtered view
    public DbSet<Admin> Admins { get; set; } = null!;          // Filtered view
    public DbSet<Course> Courses { get; set; } = null!;
    public DbSet<Enrollment> Enrollments { get; set; } = null!;
    public DbSet<GradeRecord> Grades { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── TPH (Table-Per-Hierarchy) for Person ────────────────────────
        // Single "People" table with a Discriminator column
        modelBuilder.Entity<Person>()
            .HasDiscriminator<string>("Discriminator")
            .HasValue<Student>("Student")
            .HasValue<Faculty>("Faculty")
            .HasValue<Admin>("Admin");

        // Email index (not unique — students can share email across semesters per existing logic)
        modelBuilder.Entity<Person>()
            .HasIndex(p => p.Email);

        // ── Student → Enrollments (one-to-many) ─────────────────────────
        modelBuilder.Entity<Student>()
            .HasMany(s => s.Enrollments)
            .WithOne(e => e.Student!)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Course → Enrollments (one-to-many) ──────────────────────────
        modelBuilder.Entity<Course>()
            .HasMany(c => c.Enrollments)
            .WithOne(e => e.Course!)
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Course → Faculty (optional one-to-many) ─────────────────────
        modelBuilder.Entity<Course>()
            .HasOne(c => c.AssignedFaculty)
            .WithMany(f => f.AssignedCourses)
            .HasForeignKey(c => c.FacultyId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // ── GradeRecord → Student (many-to-one) ────────────────────────
        modelBuilder.Entity<GradeRecord>()
            .HasOne(g => g.Student)
            .WithMany()
            .HasForeignKey(g => g.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── GradeRecord → Course (many-to-one) ─────────────────────────
        modelBuilder.Entity<GradeRecord>()
            .HasOne(g => g.Course)
            .WithMany()
            .HasForeignKey(g => g.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Notification → Person (many-to-one) ────────────────────────
        modelBuilder.Entity<Notification>()
            .HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Enrollment: store enum as string for readability ────────────
        modelBuilder.Entity<Enrollment>()
            .Property(e => e.State)
            .HasConversion<string>();

        // ── Notification: store enum as string ──────────────────────────
        modelBuilder.Entity<Notification>()
            .Property(n => n.Type)
            .HasConversion<string>();
    }
}
