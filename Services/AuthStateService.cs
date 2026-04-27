using EduConnect.Interfaces;
using EduConnect.Models;

namespace EduConnect.Services;

// ── InMemoryRepository<T> ──────────────────────────────────────────────────
// OCP: no changes needed when a new entity type is added
// DIP: registered as interface in Program.cs — components never call 'new' on this
public class InMemoryRepository<T> : IRepository<T> where T : class
{
    private readonly List<T> _store = new();
    private readonly Func<T, Guid> _idSelector;

    public InMemoryRepository(Func<T, Guid> idSelector) => _idSelector = idSelector;

    public List<T> GetAll() => _store.ToList();

    public T? GetById(Guid id) => _store.FirstOrDefault(x => _idSelector(x) == id);

    public void Add(T entity) => _store.Add(entity);

    public void Update(T entity)
    {
        var index = _store.FindIndex(x => _idSelector(x) == _idSelector(entity));
        if (index >= 0) _store[index] = entity;
    }

    public void Delete(Guid id) => _store.RemoveAll(x => _idSelector(x) == id);
}

// ── AuthStateService ───────────────────────────────────────────────────────
// SRP: only manages login state
// Scoped service — one instance per Blazor circuit (simulates session)
public class AuthStateService
{
    public Person? CurrentUser { get; private set; }

    // Components subscribe to this event in OnInitialized, unsubscribe in Dispose
    public event Action? OnAuthStateChanged;

    private readonly List<Person> _users;

    public AuthStateService()
    {
        // Seed mock users — one of each role
        _users = new List<Person>
        {
            new Admin   { Id = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001"),
                          FullName = "Dr. Ayesha Khan", Email = "admin@au.edu.pk",  Password = "admin123" },
            new Faculty { Id = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000001"),
                          FullName = "Prof. Tariq Shah", Email = "tariq@au.edu.pk", Password = "faculty123",
                          Department = "Computer Science" },
            new Student { Id = Guid.Parse("cccccccc-0000-0000-0000-000000000001"),
                          FullName = "M. Usaidullah Rehan",  Email = "student@au.edu.pk", Password = "student123",
                          Semester = 6, CGPA = 3.5 },
        };
    }

    // Returns null on failure (bad credentials)
    public Person? Login(string email, string password)
    {
        var user = _users.FirstOrDefault(u =>
            u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && u.Password == password);
        if (user is null) return null;
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
    public List<Person> GetAllUsers() => _users.ToList();
    public void RegisterUser(Person user) => _users.Add(user);
}
