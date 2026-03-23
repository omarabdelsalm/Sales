using Microsoft.EntityFrameworkCore;
using Sales.Web.Components;
using Sales.Shared.Data;
using Sales.Shared.Services;
using Sales.Web.Services;
using Sales.Shared.Models;

var builder = WebApplication.CreateBuilder(args);

// Add Razor Components
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers(); // لدعم الـ API للربط مع الموبايل

// Add EF Core with Database Switching (SQLite for Dev, SQL Server for Prod)
if (builder.Environment.IsDevelopment())
{
    var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "sales.db");
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite($"Data Source={dbPath}"));
}
else
{
    // عند الرفع للإنتاج، استخدم SQL Server من ملف الإعدادات
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// Auth service (scoped so each user session has its own instance)
builder.Services.AddScoped<IDataService, DbDataService>();
builder.Services.AddScoped<AuthService>();

// Device-specific services
builder.Services.AddSingleton<IFormFactor, FormFactor>();

// File upload service (uses IWebHostEnvironment to resolve wwwroot/uploads path)
builder.Services.AddScoped<IFileUploadService, WebFileUploadService>();

var app = builder.Build();

// Auto-migrate database on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    // ---- Seed Default Admin ----
    var adminEmail = "admin@sales.com";
    if (!db.Users.Any(u => u.Role == Sales.Shared.Models.UserRole.Admin))
    {
        var existingUser = db.Users.FirstOrDefault(u => u.Email == adminEmail);
        if (existingUser != null)
        {
            // If user exists with this email but isn't admin, promote them
            existingUser.Role = UserRole.Admin;
        }
        else
        {
            // Create new admin
            var admin = new AppUser
            {
                FullName = "مدير النظام",
                Email = adminEmail,
                PasswordHash = "8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918", // Hash for 'admin'
                Role = UserRole.Admin,
                CreatedAt = DateTime.UtcNow
            };
            db.Users.Add(admin);
        }
        db.SaveChanges();
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapControllers(); // تفعيل روابط الـ API
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(Sales.Shared._Imports).Assembly);

app.Run();
