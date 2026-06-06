using Microsoft.EntityFrameworkCore;
using EduConnect.Data;
using EduConnect.Interfaces;
using EduConnect.Models;

namespace EduConnect.Services;

// ── AuthStateService ───────────────────────────────────────────────────────
// SRP: only manages login state
// Scoped service — one instance per Blazor circuit (simulates session)
public class AuthStateService
{
    public Person? CurrentUser { get; private set; }

    // Components subscribe to this event in OnInitialized, unsubscribe in Dispose
    public event Action? OnAuthStateChanged;

    private readonly EduConnectDbContext _db;

    public AuthStateService(EduConnectDbContext db)
    {
        _db = db;
    }

    // Returns null on failure (bad credentials)
    public Person? Login(string email, string password)
    {
        // Query all People (Student, Faculty, Admin via TPH) from the database
        var user = _db.People
            .FirstOrDefault(u =>
                u.Email.ToLower() == email.ToLower() && u.Password == password);

        if (user is null) return null;

        // If the user is a Student, eagerly load their Enrollments
        if (user is Student student)
        {
            _db.Entry(student).Collection(s => s.Enrollments).Load();
        }

        CurrentUser = user;
        OnAuthStateChanged?.Invoke(); // fire event → NavBar re-renders
        return user;
    }

    public void Logout()
    {
        CurrentUser = null;
        OnAuthStateChanged?.Invoke();
    }

    public bool IsAuthenticated => CurrentUser is not null;
    public bool IsAdmin    => CurrentUser?.GetRole() == UserRole.Admin;
    public bool IsFaculty  => CurrentUser?.GetRole() == UserRole.Faculty;
    public bool IsStudent  => CurrentUser?.GetRole() == UserRole.Student;

    // Expose user list so other services can find users for notifications
    public List<Person> GetAllUsers()
    {
        return _db.People.AsNoTracking().ToList();
    }

    public void RegisterUser(Person user)
    {
        // No-op: the user is already added via StudentService.Add() which writes to the DB.
        // This method exists to maintain interface compatibility with AddStudent.razor.
        // In the DB-backed world, StudentService.Add already persists the student to the
        // People table (via TPH), so they're immediately available for login.
    }
}
