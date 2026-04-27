using EduConnect.Interfaces;

namespace EduConnect.Models;

// OCP: new roles (e.g. Librarian) can extend Person without changing existing code
// LSP: Student/Faculty/Admin can replace Person anywhere it's expected
public abstract class Person
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = "password123"; // mock auth
    public abstract UserRole GetRole(); // LSP: each subtype returns correct role
}

// ── Student ────────────────────────────────────────────────────────────────
public class Student : Person, IValidatable
{
    public int Semester { get; set; } = 1;    // 1–8
    public double CGPA { get; set; } = 0.0;
    public List<Enrollment> Enrollments { get; set; } = new();

    public override UserRole GetRole() => UserRole.Student;

    // IValidatable: SRP — validation logic lives here, not in .razor
    public List<string> Validate()
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(FullName))  errors.Add("Full name is required.");
        if (string.IsNullOrWhiteSpace(Email))      errors.Add("Email is required.");
        if (!Email.Contains('@'))                   errors.Add("Email must be valid.");
        if (Semester < 1 || Semester > 8)          errors.Add("Semester must be 1–8.");
        if (CGPA < 0.0 || CGPA > 4.0)             errors.Add("CGPA must be between 0.0 and 4.0.");
        return errors;
    }
}

// ── Faculty ────────────────────────────────────────────────────────────────
public class Faculty : Person
{
    public string Department { get; set; } = string.Empty;
    public List<Course> AssignedCourses { get; set; } = new();
    public override UserRole GetRole() => UserRole.Faculty;
}

// ── Admin ──────────────────────────────────────────────────────────────────
public class Admin : Person
{
    public override UserRole GetRole() => UserRole.Admin;
}
