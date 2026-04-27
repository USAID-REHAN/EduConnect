// Program.cs — entry point, registers all services via DI
// DIP: components depend on interfaces, not concrete classes

using EduConnect.Interfaces;
using EduConnect.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Blazor Server setup ────────────────────────────────────────────────────
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// ── Register services as Singletons (in-memory store = one shared instance) ─
// DIP: IStudentService injected where needed — not 'new StudentService()'
builder.Services.AddSingleton<AuthStateService>();
builder.Services.AddSingleton<INotificationService, NotificationService>();
builder.Services.AddSingleton<IStudentService,      StudentService>();
builder.Services.AddSingleton<ICourseService,       CourseService>();
builder.Services.AddSingleton<IGradeService,        GradeService>();

var app = builder.Build();

// ── Middleware ──────────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
