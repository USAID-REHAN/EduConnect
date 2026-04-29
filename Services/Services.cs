using EduConnect.Exceptions;
using EduConnect.Interfaces;
using EduConnect.Models;

namespace EduConnect.Services;

// ── StudentService ─────────────────────────────────────────────────────────
// SRP: only manages student CRUD + search
// DIP: injected via IStudentService everywhere (never newed up in components)
public class StudentService : IStudentService
{
    private readonly InMemoryRepository<Student> _repo;
    public event Action? OnStudentUpdated; // components can react to changes

    public StudentService()
    {
        _repo = new InMemoryRepository<Student>(s => s.Id);
        // Seed some demo students
        Add(new Student { FullName = "M. Usaidullah Rehan", Email = "usaidullah@au.edu.pk", Semester = 6, CGPA = 3.5 });
        Add(new Student { FullName = "M. Ayan Hamdani Rehman", Email = "ayan@au.edu.pk", Semester = 4, CGPA = 3.1 });
        Add(new Student { FullName = "Ahmed Baig", Email = "ahmed@au.edu.pk", Semester = 2, CGPA = 2.8 });
        Add(new Student { FullName = "Syed Shadan Raza", Email = "shadan@au.edu.pk", Semester = 7, CGPA = 3.9 });
        Add(new Student { FullName = "Omar Farooq", Email = "omar@au.edu.pk", Semester = 3, CGPA = 2.5 });
    }

    public List<Student> GetAll() => _repo.GetAll();
    public Student? GetById(Guid id) => _repo.GetById(id);

    // ── BUG 2 FIX: duplicate email guard ────────────────────────────────────
    public void Add(Student entity)
    {
        var exists = _repo.GetAll().Any(s =>
        s.Email.Equals(entity.Email, StringComparison.OrdinalIgnoreCase)
        && s.Semester == entity.Semester
    );

        if (exists)
            throw new Exception($"Student already exists in Semester {entity.Semester}.");

        _repo.Add(entity);
    }

    public void Update(Student entity) { _repo.Update(entity); OnStudentUpdated?.Invoke(); }

    public void Delete(Guid id)
    {
        var student = GetById(id) ?? throw new Exception("Student not found.");
        // Business rule: cannot delete if active enrollments exist
        if (student.Enrollments.Any(e => e.State == EnrollmentState.Active))
            throw new StudentHasActiveEnrollmentsException(student.FullName);
        _repo.Delete(id);
    }

    // Live search — used with two-way binding on Student List page
    public List<Student> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return GetAll();
        return GetAll()
            .Where(s => s.FullName.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
}

// ── CourseService ──────────────────────────────────────────────────────────
// SRP: handles course CRUD and enrollment/drop logic
public class CourseService : ICourseService
{
    private readonly InMemoryRepository<Course> _repo;
    private readonly INotificationService _notifService;
    private readonly IStudentService _studentService;

    public event Action? OnEnrollmentChanged;

    // ── BUG 3 FIX: dedicated course-list event ──────────────────────────────
    public event Action? OnCourseChanged;

    public CourseService(INotificationService notifService, IStudentService studentService)
    {
        _notifService = notifService;
        _studentService = studentService;
        _repo = new InMemoryRepository<Course>(c => c.Id);

        // Seed demo courses
        var faculty1Id = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000001");
        _repo.Add(new Course { Code = "CS-284", Title = "Visual Programming", CreditHours = 3, MaxCapacity = 30, FacultyId = faculty1Id });
        _repo.Add(new Course { Code = "CS-301", Title = "Data Structures", CreditHours = 3, MaxCapacity = 25, FacultyId = faculty1Id });
        _repo.Add(new Course { Code = "CS-401", Title = "Software Engineering", CreditHours = 3, MaxCapacity = 35, FacultyId = faculty1Id });
        _repo.Add(new Course { Code = "MT-201", Title = "Calculus-II", CreditHours = 3, MaxCapacity = 40 });
        _repo.Add(new Course { Code = "CS-302", Title = "Computer Networks", CreditHours = 3, MaxCapacity = 30 });
    }

    public List<Course> GetAll() => _repo.GetAll();
    public Course? GetById(Guid id) => _repo.GetById(id);

    public void Add(Course entity)
    {
        _repo.Add(entity);
        OnCourseChanged?.Invoke();
    }

    public void Update(Course entity)
    {
        _repo.Update(entity);
        OnCourseChanged?.Invoke();
    }

    public void Delete(Guid id)
    {
        _repo.Delete(id);
        OnCourseChanged?.Invoke();
    }

    public void EnrollStudent(Guid studentId, Guid courseId)
    {
        var course = GetById(courseId) ?? throw new Exception("Course not found.");

        if (course.Status == EnrollmentStatus.Full)
            throw new CourseFullException(course.Title);

        // Business rule: can't re-enroll if dropped this semester (checked via Course side)
        if (course.Enrollments.Any(e => e.StudentId == studentId && e.DroppedThisSemester))
            throw new Exception("You cannot re-enroll in a course you dropped this semester.");

        // Already enrolled? (checked via Course side)
        if (course.Enrollments.Any(e => e.StudentId == studentId && e.State == EnrollmentState.Active))
            throw new Exception("Already enrolled in this course.");

        var enrollment = new Enrollment { StudentId = studentId, CourseId = courseId };

        // Add to course enrollments
        course.Enrollments.Add(enrollment);

        // Safely attempt to add to student enrollments if the student actually exists in the StudentService
        var student = _studentService.GetById(studentId);
        if (student != null)
        {
            student.Enrollments.Add(enrollment);
        }

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
        var course = GetById(courseId) ?? throw new Exception("Course not found.");

        // Fetch enrollment purely from the course side
        var courseEnrollment = course.Enrollments
            .FirstOrDefault(e => e.StudentId == studentId && e.State == EnrollmentState.Active)
            ?? throw new Exception("Enrollment not found.");

        if (courseEnrollment.State != EnrollmentState.Active)
            throw new Exception("Only active courses can be dropped.");

        courseEnrollment.State = EnrollmentState.Dropped;
        courseEnrollment.DroppedThisSemester = true;

        // Safely attempt to update student side if the student actually exists in the StudentService
        var student = _studentService.GetById(studentId);
        if (student != null)
        {
            var studentEnrollment = student.Enrollments
                .FirstOrDefault(e => e.CourseId == courseId && e.State == EnrollmentState.Active);

            if (studentEnrollment != null)
            {
                studentEnrollment.State = EnrollmentState.Dropped;
                studentEnrollment.DroppedThisSemester = true;
            }
        }

        OnEnrollmentChanged?.Invoke();
    }

    // ── BUG 1 FIX: query Course.Enrollments — not the student object ─────────
    public List<Course> GetAvailableCourses(Guid studentId)
    {
        return GetAll().Where(c =>
            c.Status != EnrollmentStatus.Full &&
            !c.Enrollments.Any(e => e.StudentId == studentId && e.State == EnrollmentState.Active)
        ).ToList();
    }

    public List<Course> GetEnrolledCourses(Guid studentId)
    {
        return GetAll().Where(c =>
            c.Enrollments.Any(e => e.StudentId == studentId && e.State == EnrollmentState.Active)
        ).ToList();
    }

    public List<Course> GetFacultyCourses(Guid facultyId)
        => GetAll().Where(c => c.FacultyId == facultyId).ToList();
}

// ── GradeService ───────────────────────────────────────────────────────────
// SRP: only handles grades — not students, not courses
public class GradeService : IGradeService
{
    private readonly List<GradeRecord> _grades = new();
    private readonly INotificationService _notifService;
    private readonly IStudentService _studentService;
    private readonly ICourseService _courseService;

    public GradeService(
        INotificationService notifService,
        IStudentService studentService,
        ICourseService courseService)
    {
        _notifService = notifService;
        _studentService = studentService;
        _courseService = courseService;
    }

    public void SubmitGrade(GradeRecord record)
    {
        var existing = _grades.FirstOrDefault(g =>
            g.StudentId == record.StudentId && g.CourseId == record.CourseId);

        if (existing != null)
        {
            existing.Marks = record.Marks;
            existing.CreditHours = record.CreditHours;
        }
        else
        {
            _grades.Add(record);
        }

        // Update student CGPA after grade submission
        var student = _studentService.GetById(record.StudentId);
        if (student != null)
            student.CGPA = ComputeCGPA(record.StudentId);

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
        => _grades.Where(g => g.StudentId == studentId).ToList();

    public List<GradeRecord> GetGradesForCourse(Guid courseId)
        => _grades.Where(g => g.CourseId == courseId).ToList();

    public double ComputeCGPA(Guid studentId)
    {
        var records = GetGradesForStudent(studentId).Where(g => g.Marks >= 0).ToList();
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
    private readonly List<Notification> _notifications = new();

    public event Action<Notification>? OnNewNotification;

    public void Send(Notification notification)
    {
        _notifications.Add(notification);
        OnNewNotification?.Invoke(notification);
    }

    public List<Notification> GetForUser(Guid userId)
        => _notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToList();

    public void MarkRead(Guid notificationId)
    {
        var notif = _notifications.FirstOrDefault(n => n.Id == notificationId);
        if (notif != null) notif.IsRead = true;
    }
}