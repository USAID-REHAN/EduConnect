namespace EduConnect.Models;

// User roles — drives dashboard views and route guards
public enum UserRole { Admin, Faculty, Student }

// Course capacity status — computed property on Course
public enum EnrollmentStatus { Open, AlmostFull, Full }

// Whether a student is still in a course this semester
public enum EnrollmentState { Active, Dropped }

// For the NotificationService event system (Module 5)
public enum NotificationType { Enrollment, GradePosted, Announcement }

// Used by AlertBox.razor to pick Bootstrap color class
public enum AlertType { Success, Warning, Error, Info }
