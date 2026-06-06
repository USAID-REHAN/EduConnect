using System.ComponentModel.DataAnnotations.Schema;
using EduConnect.Interfaces;

namespace EduConnect.Models;

// ── Course ─────────────────────────────────────────────────────────────────
public class Course
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int CreditHours { get; set; } = 3;
    public int MaxCapacity { get; set; } = 30;
    public Guid? FacultyId { get; set; }
    public List<Enrollment> Enrollments { get; set; } = new();

    // Navigation property for Faculty assignment
    [ForeignKey("FacultyId")]
    public Faculty? AssignedFaculty { get; set; }

    // Computed — no setter, pure logic (OCP: add more statuses without breaking this)
    [NotMapped]
    public int EnrolledCount => Enrollments.Count(e => e.State == EnrollmentState.Active);

    [NotMapped]
    public EnrollmentStatus Status => EnrolledCount >= MaxCapacity ? EnrollmentStatus.Full
        : EnrolledCount >= MaxCapacity * 0.8 ? EnrollmentStatus.AlmostFull
        : EnrollmentStatus.Open;

    // For progress bar in CourseCard (0–100)
    [NotMapped]
    public int EnrollmentPercent => MaxCapacity == 0 ? 0 : (int)((double)EnrolledCount / MaxCapacity * 100);
}

// ── Enrollment ─────────────────────────────────────────────────────────────
public class Enrollment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid StudentId { get; set; }
    public Guid CourseId { get; set; }
    public EnrollmentState State { get; set; } = EnrollmentState.Active;
    public DateTime EnrolledAt { get; set; } = DateTime.Now;
    public bool DroppedThisSemester { get; set; } = false; // enforces drop re-enroll rule

    // Navigation properties for EF Core relationships
    [ForeignKey("StudentId")]
    public Student? Student { get; set; }

    [ForeignKey("CourseId")]
    public Course? Course { get; set; }
}

// ── GradeRecord ────────────────────────────────────────────────────────────
public class GradeRecord : IValidatable
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid StudentId { get; set; }
    public Guid CourseId { get; set; }
    public int CreditHours { get; set; } = 3; // copied from course at submission time
    public double Marks { get; set; } = -1;   // -1 = not yet graded

    // Navigation properties for EF Core relationships
    [ForeignKey("StudentId")]
    public Student? Student { get; set; }

    [ForeignKey("CourseId")]
    public Course? Course { get; set; }

    // Computed letter grade — SRP: grade logic stays in the model
    [NotMapped]
    public string LetterGrade => Marks switch
    {
        >= 85 => "A",
        >= 70 => "B",
        >= 55 => "C",
        >= 45 => "D",
        >= 0  => "F",
        _     => "N/A"
    };

    // 4.0 grade points used for CGPA calculation
    [NotMapped]
    public double GradePoint => LetterGrade switch
    {
        "A" => 4.0, "B" => 3.0, "C" => 2.0, "D" => 1.0, _ => 0.0
    };

    // CSS class for conditional row coloring in GradeTable
    [NotMapped]
    public string RowCssClass => LetterGrade switch
    {
        "A" or "B" => "grade-high",
        "C" or "D" => "grade-mid",
        "F"        => "grade-low",
        _          => ""
    };

    // IValidatable
    public List<string> Validate()
    {
        var errors = new List<string>();
        if (Marks < 0 || Marks > 100)
            errors.Add("Marks must be between 0 and 100.");
        return errors;
    }
}

// ── Notification ───────────────────────────────────────────────────────────
public class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation property for EF Core relationship
    [ForeignKey("UserId")]
    public Person? User { get; set; }
}
