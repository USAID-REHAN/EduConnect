using EduConnect.Models;

namespace EduConnect.Interfaces;

// ── IRepository<T> ─────────────────────────────────────────────────────────
// OCP: adding new entity types (e.g. Course) doesn't change this interface
// LSP: IRepository<Student> and IRepository<Course> are interchangeable where IRepository<T> is expected
public interface IRepository<T>
{
    List<T> GetAll();
    T? GetById(Guid id);
    void Add(T entity);
    void Update(T entity);
    void Delete(Guid id);
}

// ── IStudentService ────────────────────────────────────────────────────────
// ISP: student-only methods — no grade methods here (those are in IGradeService)
public interface IStudentService : IRepository<Student>
{
    List<Student> Search(string query); // live search by name
}

// ── ICourseService ─────────────────────────────────────────────────────────
// ISP: course + enrollment logic — separated from grading
public interface ICourseService : IRepository<Course>
{
    void EnrollStudent(Guid studentId, Guid courseId);   // throws CourseFullException if full
    void DropCourse(Guid studentId, Guid courseId);      // enforces business rule in service
    List<Course> GetAvailableCourses(Guid studentId);    // not-full, not-enrolled
    List<Course> GetEnrolledCourses(Guid studentId);
    List<Course> GetFacultyCourses(Guid facultyId);
}

// ── IGradeService ──────────────────────────────────────────────────────────
// ISP: grading is fully separate from student/course management
public interface IGradeService
{
    void SubmitGrade(GradeRecord record);
    List<GradeRecord> GetGradesForStudent(Guid studentId);
    List<GradeRecord> GetGradesForCourse(Guid courseId);
    double ComputeCGPA(Guid studentId);                  // weighted average on 4.0 scale
    void MarkNotificationRead(Guid notificationId);
}

// ── INotificationService ──────────────────────────────────────────────────
// SRP: only manages notifications — nothing else
public interface INotificationService
{
    event Action<Notification> OnNewNotification;        // C# event — not polling
    void Send(Notification notification);
    List<Notification> GetForUser(Guid userId);
    void MarkRead(Guid notificationId);
}
