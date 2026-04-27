namespace EduConnect.Interfaces;

// ISP: only classes that need validation implement this
// Used by: Student, GradeRecord, and form models
public interface IValidatable
{
    List<string> Validate();   // returns list of error messages (empty = valid)
}
