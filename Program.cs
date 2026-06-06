// Program.cs — entry point, registers all services via DI
// DIP: components depend on interfaces, not concrete classes

using Microsoft.EntityFrameworkCore;
using EduConnect.Data;
using EduConnect.Interfaces;
using EduConnect.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Blazor Server setup ────────────────────────────────────────────────────
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// ── Database (EF Core + SQLite) ────────────────────────────────────────────
builder.Services.AddDbContext<EduConnectDbContext>(options =>
    options.UseSqlite("Data Source=educonnect.db"));

// ── Register services ─────────────────────────────────────────────────────
// All services are Scoped: each Blazor circuit gets its own instance + DbContext
// DIP: IStudentService injected where needed — not 'new StudentService()'
builder.Services.AddScoped<AuthStateService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IGradeService, GradeService>();

var app = builder.Build();

// ── Create database & seed demo data ───────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EduConnectDbContext>();
    db.Database.EnsureCreated();    // Creates tables from model if DB doesn't exist
    DbSeeder.SeedIfEmpty(db);       // Seed demo data only on first run
}

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
