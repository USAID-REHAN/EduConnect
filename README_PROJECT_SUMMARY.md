**Project Summary — EduConnect**

This file gives a one-line, ordered summary of each important source file and a concise mapping of architectural/OOP concepts to their concrete implementations in the codebase.

**File Structure (one-line per file)**
- **[_Imports.razor](_Imports.razor)**: Central Razor imports used across components and pages to reduce repeated `@using` and `@namespace` directives.
- **[App.razor](App.razor)**: Root Blazor application component that sets the router and layout for the SPA.
- **[EduConnect.sln](EduConnect.sln)**: Solution file containing the EduConnect project.
- **[EduConnect.csproj](EduConnect.csproj)**: Project file describing build settings, target framework, and package references.
- **[Program.cs](Program.cs)**: Application entry point; configures services, DI, routing and Blazor server endpoints.
- **[README.md](README.md)**: Project README (existing) with higher-level project instructions and notes.

**Folders and their files**
- **[Components/AlertBox.razor](Components/AlertBox.razor)**: Reusable alert UI component that accepts `Message` and `Type` and renders contextual icon and class.
- **[Components/AuthGuard.razor](Components/AuthGuard.razor)**: Page-level guard component that enforces role-based access control around child content.
- **[Components/ConfirmDialog.razor](Components/ConfirmDialog.razor)**: Generic confirm dialog used by destructive actions requiring user confirmation.
- **[Components/CourseCard.razor](Components/CourseCard.razor)**: Visual card representing a `Course`, showing enrollment progress and enroll/drop actions.
- **[Components/GradeTable.razor](Components/GradeTable.razor)**: Table UI that displays `GradeRecord` rows with conditional row styling.
- **[Components/LoadingSpinner.razor](Components/LoadingSpinner.razor)**: Small spinner component for async/loading states.
- **[Components/NotificationBell.razor](Components/NotificationBell.razor)**: Bell icon with dropdown showing user notifications from `NotificationService`.
- **[Components/StudentCard.razor](Components/StudentCard.razor)**: Compact student summary card used across lists and dashboards.

- **[Exceptions/Exceptions.cs](Exceptions/Exceptions.cs)**: Custom exception classes (domain-specific business rule exceptions) used by services for clearer error semantics.

- **[Interfaces/Interfaces.cs](Interfaces/Interfaces.cs)**: Service and repository interface definitions (`IStudentService`, `ICourseService`, etc.) used for DI and DIP.
- **[Interfaces/IValidatable.cs](Interfaces/IValidatable.cs)**: `IValidatable` interface implemented by models to centralize validation logic.

- **[Layout/EmptyLayout.razor](Layout/EmptyLayout.razor)**: Minimal layout for pages like `Login` that need no nav chrome.
- **[Layout/MainLayout.razor](Layout/MainLayout.razor)**: Main app shell including `NavBar` and standard page arrangement.
- **[Layout/NavBar.razor](Layout/NavBar.razor)**: Top navigation component that subscribes to `AuthStateService` to update on login/logout.

- **[Models/Enums.cs](Models/Enums.cs)**: Enum definitions for `UserRole`, `AlertType`, `EnrollmentState`, `NotificationType`, etc.
- **[Models/Models.cs](Models/Models.cs)**: Domain models such as `Course`, `Enrollment`, `GradeRecord`, and `Notification` with computed properties and validation.
- **[Models/Person.cs](Models/Person.cs)**: Abstract `Person` base and concrete `Student`, `Faculty`, `Admin` implementations (validation on `Student`).

- **[Pages/_Host.cshtml](Pages/_Host.cshtml)**: Server host page for Blazor Server, bootstraps the Blazor circuit.
- **[Pages/Index.razor](Pages/Index.razor)**: Landing page / home route.
- **[Pages/Login.razor](Pages/Login.razor)**: Public login page with two-way binding for inputs and `AuthStateService` integration.
- **[Pages/Dashboard.razor](Pages/Dashboard.razor)**: Main authenticated landing dashboard showing cards and user-specific data.
- **[Pages/CourseCatalog.razor](Pages/CourseCatalog.razor)**: Lists available courses and uses `CourseCard` for each.
- **[Pages/NotificationsPage.razor](Pages/NotificationsPage.razor)**: Page showing user's notifications using `NotificationService`.
- **[Pages/Unauthorized.razor](Pages/Unauthorized.razor)**: Displayed when a user tries to access a guarded route.

- **[Pages/Admin/AdminNotifications.razor](Pages/Admin/AdminNotifications.razor)**: Admin-facing notifications overview.
- **[Pages/Admin/CourseManagement.razor](Pages/Admin/CourseManagement.razor)**: CRUD UI for courses.
- **[Pages/Admin/GradeReport.razor](Pages/Admin/GradeReport.razor)**: Grade summaries and reporting tools.
- **[Pages/Admin/Students/StudentList.razor](Pages/Admin/Students/StudentList.razor)**: Student management list with live search (two-way binding) and actions.
- **[Pages/Admin/Students/AddStudent.razor](Pages/Admin/Students/AddStudent.razor)**: Form to add new students; uses `IStudentService.Add`.
- **[Pages/Admin/Students/EditStudent.razor](Pages/Admin/Students/EditStudent.razor)**: Edit form that preloads student (one-way load) and uses two-way `@bind` for edits.
- **[Pages/Admin/Students/StudentDetail.razor](Pages/Admin/Students/StudentDetail.razor)**: Single-student details view and related enrollments.

- **[Pages/Faculty/FacultyCourses.razor](Pages/Faculty/FacultyCourses.razor)**: Faculty-facing list of assigned courses and grade submission links.
- **[Pages/Faculty/GradeSubmission.razor](Pages/Faculty/GradeSubmission.razor)**: Grade submission form that calls `IGradeService.SubmitGrade`.

- **[Pages/Student/EnrollPage.razor](Pages/Student/EnrollPage.razor)**: Student UI to enroll/drop courses using `ICourseService`.
- **[Pages/Student/StudentGrades.razor](Pages/Student/StudentGrades.razor)**: Student-facing grade listing using `GradeTable`.

- **[Properties/launchSettings.json](Properties/launchSettings.json)**: Local launch configuration for the app (profiles, URLs).

- **[Services/AuthStateService.cs](Services/AuthStateService.cs)**: Scoped service tracking current user, exposes `OnAuthStateChanged` event and login/logout logic.
- **[Services/Services.cs](Services/Services.cs)**: Concrete services — `StudentService`, `CourseService`, `GradeService`, `NotificationService` with in-memory repositories and domain events.

- **[wwwroot/css/app.css](wwwroot/css/app.css)**: Application stylesheet with design tokens and component styles.

**Concepts & Their Implementations (concise mapping)**
- **One-way data binding**: Data loaded once into a local model/copy using lifecycle methods (example: `OnParametersSet()` in [Pages/Admin/Students/EditStudent.razor](Pages/Admin/Students/EditStudent.razor) creates a copy of the stored `Student` to display initial values).
- **Two-way data binding**: `@bind` is used for form inputs to update model properties in real time (examples: [Pages/Login.razor](Pages/Login.razor) binds `_email`/`_password`; [Pages/Admin/Students/EditStudent.razor](Pages/Admin/Students/EditStudent.razor) binds `_student.FullName`, `_student.Email`, `_student.Semester`).
- **Composition (UI & components)**: Pages compose small components like `AlertBox`, `CourseCard`, `GradeTable` to build pages (see [Pages/Admin/Students/EditStudent.razor](Pages/Admin/Students/EditStudent.razor) using `AlertBox` and `AuthGuard`).
- **Aggregation (domain objects)**: Domain models aggregate collections — `Course.Enrollments` and `Student.Enrollments` store `Enrollment` objects (see [Models/Models.cs](Models/Models.cs) and [Models/Person.cs](Models/Person.cs)); services synchronize both sides when needed.
- **Abstraction**: `Person` is an abstract base class with `GetRole()` (see [Models/Person.cs](Models/Person.cs)), enabling `Student`/`Faculty`/`Admin` polymorphism and Liskov Substitution.
- **Routing**: Razor pages declare routes with `@page` directives (e.g., [Pages/Login.razor](Pages/Login.razor), [Pages/Admin/Students/EditStudent.razor](Pages/Admin/Students/EditStudent.razor)) and `Program.cs` wires Blazor endpoints (`MapBlazorHub`, `MapFallbackToPage("/_Host")`).
- **Dependency Injection & DIP**: `Program.cs` registers interfaces and concrete services (`IStudentService` → `StudentService`, `AuthStateService` scoped) so components depend on interfaces, not `new` (see [Program.cs](Program.cs)).
- **SOLID examples in code**:
  - **S (SRP)**: Each service has a single responsibility (e.g., `NotificationService` handles notifications only — see [Services/Services.cs](Services/Services.cs)).
  - **O (OCP)**: Models and services are extendable without modifying existing callers (e.g., adding a new `UserRole` or course status fits existing patterns; comments in models indicate OCP awareness).
  - **L (LSP)**: `Student`/`Faculty`/`Admin` inherit `Person` and can be used wherever `Person` is expected (see [Models/Person.cs](Models/Person.cs)).
  - **I (ISP)**: Small interfaces like `IValidatable` keep validation concerns separate from unrelated APIs (see [Interfaces/IValidatable.cs](Interfaces/IValidatable.cs)).
  - **D (DIP)**: Components receive `IStudentService`/`ICourseService` via injection (see [Program.cs](Program.cs) and uses across pages/services).
- **Events & Callbacks**: Services expose events to notify consumers — `AuthStateService.OnAuthStateChanged` (used by `NavBar`), `StudentService.OnStudentUpdated`, `CourseService.OnEnrollmentChanged`, and `NotificationService.OnNewNotification` (see [Services/AuthStateService.cs](Services/AuthStateService.cs) and [Services/Services.cs](Services/Services.cs)). Components subscribe in `OnInitialized` and unsubscribe in `Dispose`.
- **Validation & Encapsulation**: Models implement `IValidatable` (e.g., `Student.Validate()`, `GradeRecord.Validate()` in [Models/Models.cs](Models/Models.cs), [Models/Person.cs](Models/Person.cs)) to keep validation out of UI code.
- **Business Rules & Error Handling**: Services encapsulate domain rules (e.g., `CourseService.EnrollStudent` prevents re-enroll/dropping rules and throws domain-specific exceptions defined in [Exceptions/Exceptions.cs](Exceptions/Exceptions.cs)).
- **State Management (scoped vs singleton)**: `AuthStateService` is `Scoped` to simulate per-tab session state; data services are `Singleton` to represent shared in-memory stores (see registration in [Program.cs](Program.cs)).

**Quick navigation pointers**
- For auth and login flow see: [Pages/Login.razor](Pages/Login.razor) and [Services/AuthStateService.cs](Services/AuthStateService.cs).
- For student CRUD and list behaviour see: [Pages/Admin/Students/StudentList.razor](Pages/Admin/Students/StudentList.razor) and [Services/Services.cs](Services/Services.cs) (StudentService).
- For course enroll/drop and notifications see: [Services/Services.cs](Services/Services.cs) and [Components/CourseCard.razor](Components/CourseCard.razor).

If you want, I can next: (1) run a quick grep to include any files I missed, (2) add line references for key excerpts, or (3) commit this file. Which would you like? 
