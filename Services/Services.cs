using Microsoft.EntityFrameworkCore;
using EduConnect.Data;
using EduConnect.Exceptions;
using EduConnect.Interfaces;
using EduConnect.Models;

namespace EduConnect.Services;

// ── StudentService ─────────────────────────────────────────────────────────
// SRP: only manages student CRUD + search
// DIP: injected via IStudentService everywhere (never newed up in components)
public class StudentService : IStudentService
{
    private readonly EduConnectDbContext _db;
    public event Action? OnStudentUpdated; // components can react to changes

    public StudentService(EduConnectDbContext db) => _db = db;

    public List<Student> GetAll()
        => _db.Students.Include(s => s.Enrollments).AsNoTracking().ToList();

    public Student? GetById(Guid id)
        => _db.Students.Include(s => s.Enrollments).FirstOrDefault(s => s.Id == id);

    // ── BUG 2 FIX: duplicate email guard ────────────────────────────────────
    public void Add(Student entity)
    {
        var exists = _db.Students.Any(s =>
            s.Email.ToLower() == entity.Email.ToLower()
            && s.Semester == entity.Semester
        );

        if (exists)
            throw new Exception($"Student already exists in Semester {entity.Semester}.");

        _db.Students.Add(entity);
        _db.SaveChanges();
    }

    public void Update(Student entity)
    {
        var existing = _db.Students.FirstOrDefault(s => s.Id == entity.Id);
        if (existing is null) return;

        existing.FullName = entity.FullName;
        existing.Email = entity.Email;
        existing.Semester = entity.Semester;
        existing.CGPA = entity.CGPA;
        existing.Password = entity.Password;
        _db.SaveChanges();
        OnStudentUpdated?.Invoke();
    }

    public void Delete(Guid id)
    {
        var student = _db.Students.Include(s => s.Enrollments).FirstOrDefault(s => s.Id == id)
            ?? throw new Exception("Student not found.");
        // Business rule: cannot delete if active enrollments exist
        if (student.Enrollments.Any(e => e.State == EnrollmentState.Active))
            throw new StudentHasActiveEnrollmentsException(student.FullName);

        _db.Students.Remove(student);
        _db.SaveChanges();
    }

    // Live search — used with two-way binding on Student List page
    public List<Student> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return GetAll();
        return _db.Students
            .Include(s => s.Enrollments)
            .Where(s => s.FullName.Contains(query))
            .AsNoTracking()
            .ToList();
    }
}

// ── CourseService ──────────────────────────────────────────────────────────
// SRP: handles course CRUD and enrollment/drop logic
public class CourseService : ICourseService
{
    private readonly EduConnectDbContext _db;
    private readonly INotificationService _notifService;

    public event Action? OnEnrollmentChanged;

    // ── BUG 3 FIX: dedicated course-list event ──────────────────────────
    public event Action? OnCourseChanged;

    public CourseService(EduConnectDbContext db, INotificationService notifService)
    {
        _db = db;
        _notifService = notifService;
    }

    public List<Course> GetAll()
        => _db.Courses.Include(c => c.Enrollments).AsNoTracking().ToList();

    public Course? GetById(Guid id)
        => _db.Courses.Include(c => c.Enrollments).FirstOrDefault(c => c.Id == id);

    public void Add(Course entity)
    {
        _db.Courses.Add(entity);
        _db.SaveChanges();
        OnCourseChanged?.Invoke();
    }

    public void Update(Course entity)
    {
        var existing = _db.Courses.FirstOrDefault(c => c.Id == entity.Id);
        if (existing is null) return;

        existing.Code = entity.Code;
        existing.Title = entity.Title;
        existing.CreditHours = entity.CreditHours;
        existing.MaxCapacity = entity.MaxCapacity;
        existing.FacultyId = entity.FacultyId;
        _db.SaveChanges();
        OnCourseChanged?.Invoke();
    }

    public void Delete(Guid id)
    {
        var course = _db.Courses.FirstOrDefault(c => c.Id == id);
        if (course is null) return;
        _db.Courses.Remove(course);
        _db.SaveChanges();
        OnCourseChanged?.Invoke();
    }

    public void EnrollStudent(Guid studentId, Guid courseId)
    {
        var course = _db.Courses.Include(c => c.Enrollments)
            .FirstOrDefault(c => c.Id == courseId)
            ?? throw new Exception("Course not found.");

        if (course.Status == EnrollmentStatus.Full)
            throw new CourseFullException(course.Title);

        // Business rule: can't re-enroll if dropped this semester
        if (course.Enrollments.Any(e => e.StudentId == studentId && e.DroppedThisSemester))
            throw new Exception("You cannot re-enroll in a course you dropped this semester.");

        // Already enrolled?
        if (course.Enrollments.Any(e => e.StudentId == studentId && e.State == EnrollmentState.Active))
            throw new Exception("Already enrolled in this course.");

        var enrollment = new Enrollment { StudentId = studentId, CourseId = courseId };
        _db.Enrollments.Add(enrollment);
        _db.SaveChanges();

        // Fire notification event (Module 5)
        _notifService.Send(new Notification
        {
            UserId = studentId,
            Message = $"You have been enrolled in {course.Code}: {course.Title}.",
            Type = NotificationType.Enrollment
        });

        OnEnrollmentChanged?.Invoke();
    }

    public void DropCourse(Guid studentId, Guid courseId)
    {
        var course = _db.Courses.Include(c => c.Enrollments)
            .FirstOrDefault(c => c.Id == courseId)
            ?? throw new Exception("Course not found.");

        // Fetch enrollment from the course side
        var enrollment = course.Enrollments
            .FirstOrDefault(e => e.StudentId == studentId && e.State == EnrollmentState.Active)
            ?? throw new Exception("Enrollment not found.");

        if (enrollment.State != EnrollmentState.Active)
            throw new Exception("Only active courses can be dropped.");

        // Track the enrollment entity for update
        var trackedEnrollment = _db.Enrollments.FirstOrDefault(e => e.Id == enrollment.Id);
        if (trackedEnrollment is not null)
        {
            trackedEnrollment.State = EnrollmentState.Dropped;
            trackedEnrollment.DroppedThisSemester = true;
            _db.SaveChanges();
        }

        OnEnrollmentChanged?.Invoke();
    }

    // ── BUG 1 FIX: query Course.Enrollments — not the student object ─────────
    public List<Course> GetAvailableCourses(Guid studentId)
    {
        return _db.Courses.Include(c => c.Enrollments)
            .AsNoTracking()
            .AsEnumerable() // switch to client-side for computed Status property
            .Where(c =>
                c.Status != EnrollmentStatus.Full &&
                !c.Enrollments.Any(e => e.StudentId == studentId && e.State == EnrollmentState.Active))
            .ToList();
    }

    public List<Course> GetEnrolledCourses(Guid studentId)
    {
        return _db.Courses.Include(c => c.Enrollments)
            .AsNoTracking()
            .AsEnumerable()
            .Where(c =>
                c.Enrollments.Any(e => e.StudentId == studentId && e.State == EnrollmentState.Active))
            .ToList();
    }

    public List<Course> GetFacultyCourses(Guid facultyId)
        => _db.Courses.Include(c => c.Enrollments)
            .Where(c => c.FacultyId == facultyId)
            .AsNoTracking()
            .ToList();
}

// ── GradeService ───────────────────────────────────────────────────────────
// SRP: only handles grades — not students, not courses
public class GradeService : IGradeService
{
    private readonly EduConnectDbContext _db;
    private readonly INotificationService _notifService;
    private readonly IStudentService _studentService;
    private readonly ICourseService _courseService;

    public GradeService(
        EduConnectDbContext db,
        INotificationService notifService,
        IStudentService studentService,
        ICourseService courseService)
    {
        _db = db;
        _notifService = notifService;
        _studentService = studentService;
        _courseService = courseService;
    }

    public void SubmitGrade(GradeRecord record)
    {
        var existing = _db.Grades.FirstOrDefault(g =>
            g.StudentId == record.StudentId && g.CourseId == record.CourseId);

        if (existing != null)
        {
            existing.Marks = record.Marks;
            existing.CreditHours = record.CreditHours;
        }
        else
        {
            _db.Grades.Add(record);
        }

        _db.SaveChanges();

        // Update student CGPA after grade submission
        var student = _db.Students.FirstOrDefault(s => s.Id == record.StudentId);
        if (student != null)
        {
            student.CGPA = ComputeCGPA(record.StudentId);
            _db.SaveChanges();
        }

        // Fire notification to student (Module 5)
        var course = _courseService.GetById(record.CourseId);
        _notifService.Send(new Notification
        {
            UserId = record.StudentId,
            Message = $"Grades posted for {course?.Code ?? "your course"}. You received {record.LetterGrade}.",
            Type = NotificationType.GradePosted
        });
    }

    public List<GradeRecord> GetGradesForStudent(Guid studentId)
        => _db.Grades.Where(g => g.StudentId == studentId).AsNoTracking().ToList();

    public List<GradeRecord> GetGradesForCourse(Guid courseId)
        => _db.Grades.Where(g => g.CourseId == courseId).AsNoTracking().ToList();

    public double ComputeCGPA(Guid studentId)
    {
        var records = _db.Grades
            .Where(g => g.StudentId == studentId)
            .AsNoTracking()
            .ToList() // materialize to compute client-side properties
            .Where(g => g.Marks >= 0)
            .ToList();
        if (!records.Any()) return 0.0;
        double totalPoints = records.Sum(g => g.GradePoint * g.CreditHours);
        double totalHours = records.Sum(g => g.CreditHours);
        return Math.Round(totalPoints / totalHours, 2);
    }

    public void MarkNotificationRead(Guid notificationId) { /* handled by NotificationService */ }
}

// ── NotificationService ────────────────────────────────────────────────────
// SRP: only manages notifications
public class NotificationService : INotificationService
{
    private readonly EduConnectDbContext _db;

    public event Action<Notification>? OnNewNotification;

    public NotificationService(EduConnectDbContext db) => _db = db;

    public void Send(Notification notification)
    {
        _db.Notifications.Add(notification);
        _db.SaveChanges();
        OnNewNotification?.Invoke(notification);
    }

    public List<Notification> GetForUser(Guid userId)
        => _db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .AsNoTracking()
            .ToList();

    public void MarkRead(Guid notificationId)
    {
        var notif = _db.Notifications.FirstOrDefault(n => n.Id == notificationId);
        if (notif != null)
        {
            notif.IsRead = true;
            _db.SaveChanges();
        }
    }
}