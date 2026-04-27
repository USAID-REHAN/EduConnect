EduConnect — Air University Islamabad
Visual Programming Assignment 2 | Spring 2026

👥 Group Members

| Name | Roll No |
|------|---------|
| M. Usaidullah Rehan | 241815 |
| M. Ayan Hamdani Rehman | 241872 |
| Syed Shadan Raza | 241911 |

📋 Project Overview

EduConnect is a Blazor Server academic management portal built for Air University Islamabad. It supports three user roles — Admin, Faculty, and Student — each with their own dashboard and feature set. All data is stored in-memory (no database required).

🛠️ Tech Stack

| Layer | Technology |
|-------|-----------|
| Framework | ASP.NET Core 8 — Blazor Server |
| Language | C# 12 |
| UI | Custom CSS + Bootstrap 5 (grid only) |
| Data | In-memory repository (no database) |
| Auth | Mock session via `AuthStateService` |

🚀 How to Run

Prerequisites: .NET 8 SDK installed ([download](https://dotnet.microsoft.com/download))

bash
1. Extract the ZIP and navigate into the project folder
cd EduConnect

2. Restore packages
dotnet restore

3. Run
dotnet run

Then open your browser at `https://localhost:{port}` — it will redirect to the login page automatically.

🔐 Demo Credentials

| Role | Email | Password |
|------|-------|----------|
| Admin | `admin@au.edu.pk` | `admin123` |
| Faculty | `tariq@au.edu.pk` | `faculty123` |
| Student | `student@au.edu.pk` | `student123` |


📁 Folder Structure

EduConnect/
├── App.razor                        # Root router component
├── _Imports.razor                   # Global Razor usings
├── Program.cs                       # DI registration + app setup
├── EduConnect.csproj
│
├── Models/
│   ├── Enums.cs                     # UserRole, EnrollmentStatus, AlertType...
│   ├── Person.cs                    # Abstract Person → Student, Faculty, Admin
│   └── Models.cs                    # Course, Enrollment, GradeRecord, Notification
│
├── Interfaces/
│   ├── IValidatable.cs              # Validate() → List<string> errors
│   └── Interfaces.cs                # IRepository<T>, IStudentService, ICourseService,
│                                    # IGradeService, INotificationService
│
├── Exceptions/
│   └── Exceptions.cs                # CourseFullException,
│                                    # StudentHasActiveEnrollmentsException
│
├── Services/
│   ├── AuthStateService.cs          # Login/logout + seeded mock users
│   └── Services.cs                  # StudentService, CourseService,
│                                    # GradeService, NotificationService
│
├── Components/
│   ├── AlertBox.razor               # Reusable success/error/info alerts
│   ├── AuthGuard.razor              # Route protection by role
│   ├── ConfirmDialog.razor          # Delete/drop confirmation modal
│   ├── CourseCard.razor             # Course card with enroll EventCallback
│   ├── GradeTable.razor             # Grade display with conditional row colors
│   ├── LoadingSpinner.razor         # Async loading wrapper
│   ├── NotificationBell.razor       # Live bell icon with unread badge
│   └── StudentCard.razor            # Student summary card
│
├── Layout/
│   ├── MainLayout.razor             # Wraps all pages with NavBar
│   ├── EmptyLayout.razor            # Login page layout (no NavBar)
│   └── NavBar.razor                 # Role-based nav + sign out
│
├── Pages/
│   ├── _Host.cshtml                 # Blazor Server host page
│   ├── Index.razor                  # Root "/" → redirects to login
│   ├── Login.razor                  # Login with show/hide password
│   ├── Dashboard.razor              # Role-specific dashboard
│   ├── CourseCatalog.razor          # All courses (all roles)
│   ├── NotificationsPage.razor      # Full notification list
│   ├── Unauthorized.razor           # 403 page
│   │
│   ├── Admin/
│   │   ├── Students/
│   │   │   ├── StudentList.razor    # Live-search student table + delete
│   │   │   ├── AddStudent.razor     # Add student with IValidatable errors
│   │   │   ├── EditStudent.razor    # Edit student (pre-populated form)
│   │   │   └── StudentDetail.razor  # Full student profile + grade history
│   │   ├── CourseManagement.razor   # Course CRUD (inline add/edit form)
│   │   ├── GradeReport.razor        # All students sorted by CGPA
│   │   └── AdminNotifications.razor # Broadcast to roles via checkboxes
│   │
│   ├── Faculty/
│   │   ├── FacultyCourses.razor     # Assigned courses overview
│   │   └── GradeSubmission.razor    # Inline marks entry per course
│   │
│   └── Student/
│       ├── EnrollPage.razor         # Enroll/drop courses with confirmation
│       └── StudentGrades.razor      # CGPA summary + grade table
│
└── wwwroot/css/
    └── app.css                      # Full custom theme (Navy-Sapphire-White)

✅ Assignment Requirements Checklist

Object-Oriented Programming
| Requirement | Implementation |
|-------------|---------------|
| Abstract base class | `Person` (abstract) — `Student`, `Faculty`, `Admin` inherit from it |
| Method overriding | `GetRole()` overridden in each subclass |
| Liskov Substitution | All subtypes used wherever `Person` is expected |
| Encapsulation | Properties with computed getters (`EnrolledCount`, `Status`, `LetterGrade`) |

SOLID Principles
| Principle | Implementation |
|-----------|---------------|
| **S** — Single Responsibility | Each service has one job: `StudentService` manages students only, `GradeService` manages grades only |
| **O** — Open/Closed | `InMemoryRepository<T>` works for any entity without modification |
| **L** — Liskov Substitution | `IRepository<Student>` and `IRepository<Course>` are interchangeable |
| **I** — Interface Segregation | `IStudentService`, `ICourseService`, `IGradeService`, `INotificationService` are separate |
| **D** — Dependency Inversion | All components inject interfaces (`IStudentService`) — never `new StudentService()` |

Blazor Data Binding
| Type | Where Used |
|------|-----------|
| **Two-way (`@bind`)** | All form inputs, search bar, dropdowns, grade entry fields |
| **One-way (display)** | NavBar shows `Auth.CurrentUser.FullName`, Dashboard stats |
| **Event binding (`@onclick`, `@onkeypress`)** | Buttons, keyboard Enter on login |
| **EventCallback** | `CourseCard.OnEnroll`, `ConfirmDialog.OnConfirmed / OnCancelled` |

Interfaces
| Interface | Purpose |
|-----------|---------|
| `IRepository<T>` | Generic CRUD — `GetAll`, `GetById`, `Add`, `Update`, `Delete` |
| `IStudentService` | Extends `IRepository<Student>` + `Search(string query)` |
| `ICourseService` | Enrollment/drop logic + filtered course lists |
| `IGradeService` | Grade submission + CGPA computation |
| `INotificationService` | Event-driven notification broadcast |
| `IValidatable` | `Validate() → List<string>` — implemented by `Student`, `GradeRecord` |

C# Events (Module 5)
| Event | Subscriber | Trigger |
|-------|-----------|---------|
| `NotificationService.OnNewNotification` | `NotificationBell` (subscribe in `OnInitialized`, unsubscribe in `Dispose`) | Enrollment, grade post, broadcast |
| `AuthStateService.OnAuthStateChanged` | `NavBar` (same pattern) | Login / logout |
| `StudentService.OnStudentUpdated` | Available for any subscriber | Student edit saved |

Exception Handling
| Exception | Thrown When | Caught In |
|-----------|------------|-----------|
| `CourseFullException` | Student tries to enroll in a full course | `EnrollPage.razor` — shown as inline alert |
| `StudentHasActiveEnrollmentsException` | Admin tries to delete an enrolled student | `StudentList.razor` — shown as inline alert |

LINQ Usage
- `StudentService.Search()` — `Where` + `Contains` (live search)
- `GradeService.ComputeCGPA()` — weighted `Sum` / `Sum`
- `StudentDetail` — enrolled courses sorted `OrderBy(c => c.Title)`
- `GradeReport` — students sorted `OrderByDescending(s => s.CGPA)`
- `CourseService` — `GetAvailableCourses`, `GetEnrolledCourses`, `GetFacultyCourses`

🎨 Design Notes

- Color scheme: Navy `#0B1F3A` + Sapphire `#1A4B8C` + White — Air University palette
- Unique elements: Diagonal left-edge accent on NavBar, geometric circles on login background, glass-card hover lift effect, CGPA color-coded badges, grade row conditional highlighting
- Font: Inter (Google Fonts)

---

EduConnect — Visual Programming (CS-284) | Air University Islamabad | Spring 2026
