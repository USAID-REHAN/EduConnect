namespace EduConnect.Exceptions;

// Thrown when admin tries to delete a student who has active enrollments
public class StudentHasActiveEnrollmentsException : Exception
{
    public StudentHasActiveEnrollmentsException(string studentName)
        : base($"Cannot delete '{studentName}' — they have active course enrollments.") { }
}

// Thrown when a student tries to enroll in a full course
public class CourseFullException : Exception
{
    public CourseFullException(string courseTitle)
        : base($"'{courseTitle}' is at full capacity. Enrollment not allowed.") { }
}
