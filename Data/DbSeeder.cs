using EduConnect.Models;

namespace EduConnect.Data;

/// <summary>
/// Seeds demo data on first run (only if the database is empty).
/// Matches the exact same GUIDs, names, and credentials from the original
/// in-memory implementation so all existing demo workflows work identically.
/// </summary>
public static class DbSeeder
{
    public static void SeedIfEmpty(EduConnectDbContext db)
    {
        // Only seed if no people exist yet (first run or after DB reset)
        if (db.People.Any()) return;

        // ── Admin ──────────────────────────────────────────────────────
        var admin = new Admin
        {
            Id       = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001"),
            FullName = "Dr. Ayesha Khan",
            Email    = "admin@au.edu.pk",
            Password = "admin123"
        };

        // ── Faculty ────────────────────────────────────────────────────
        var faculty = new Faculty
        {
            Id         = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000001"),
            FullName   = "Prof. Tariq Shah",
            Email      = "tariq@au.edu.pk",
            Password   = "faculty123",
            Department = "Computer Science"
        };

        // ── Students ───────────────────────────────────────────────────
        // Note: the first student also has a dedicated auth login email
        // (student@au.edu.pk). We keep their primary email as usaidullah@au.edu.pk
        // and add a separate record for the demo login convenience,
        // matching the original AuthStateService behavior.
        var student1 = new Student
        {
            Id       = Guid.Parse("cccccccc-0000-0000-0000-000000000001"),
            FullName = "M. Usaidullah Rehan",
            Email    = "student@au.edu.pk",
            Password = "student123",
            Semester = 6,
            CGPA     = 3.5
        };
        var student2 = new Student
        {
            FullName = "M. Usaidullah Rehan",
            Email    = "usaidullah@au.edu.pk",
            Password = "password123",
            Semester = 6,
            CGPA     = 3.5
        };
        var student3 = new Student
        {
            FullName = "M. Ayan Hamdani Rehman",
            Email    = "ayan@au.edu.pk",
            Password = "password123",
            Semester = 4,
            CGPA     = 3.1
        };
        var student4 = new Student
        {
            FullName = "Ahmed Baig",
            Email    = "ahmed@au.edu.pk",
            Password = "password123",
            Semester = 2,
            CGPA     = 2.8
        };
        var student5 = new Student
        {
            FullName = "Syed Shadan Raza",
            Email    = "shadan@au.edu.pk",
            Password = "password123",
            Semester = 7,
            CGPA     = 3.9
        };
        var student6 = new Student
        {
            FullName = "Omar Farooq",
            Email    = "omar@au.edu.pk",
            Password = "password123",
            Semester = 3,
            CGPA     = 2.5
        };

        db.People.AddRange(admin, faculty, student1, student2, student3, student4, student5, student6);

        // ── Courses ────────────────────────────────────────────────────
        var courses = new List<Course>
        {
            new Course { Code = "CS-284", Title = "Visual Programming",   CreditHours = 3, MaxCapacity = 30, FacultyId = faculty.Id },
            new Course { Code = "CS-301", Title = "Data Structures",      CreditHours = 3, MaxCapacity = 25, FacultyId = faculty.Id },
            new Course { Code = "CS-401", Title = "Software Engineering", CreditHours = 3, MaxCapacity = 35, FacultyId = faculty.Id },
            new Course { Code = "MT-201", Title = "Calculus-II",          CreditHours = 3, MaxCapacity = 40 },
            new Course { Code = "CS-302", Title = "Computer Networks",    CreditHours = 3, MaxCapacity = 30 },
        };

        db.Courses.AddRange(courses);

        // ── Persist everything ─────────────────────────────────────────
        db.SaveChanges();
    }
}
