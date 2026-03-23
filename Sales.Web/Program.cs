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

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Add EF Core with Database Switching (SQLite for Dev, SQL Server for Prod)
if (builder.Environment.IsDevelopment())
{
    // استخدام قاعدة بيانات محلية في المجلد الرئيسي للمشروع للتطوير
    var dbPath = Path.Combine(builder.Environment.ContentRootPath, "sales.db");
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite($"Data Source={dbPath}"));
    Console.WriteLine($"[DEBUG] Using SQLite Database at: {dbPath}");
}
else
{
    // عند الرفع للإنتاج، استخدم SQL Server
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
    Console.WriteLine("[DEBUG] Using SQL Server Database");
}

// Auth service
builder.Services.AddScoped<IDataService, DbDataService>();
builder.Services.AddScoped<AuthService>();

// Device-specific services
builder.Services.AddSingleton<IFormFactor, FormFactor>();

// File upload service
builder.Services.AddScoped<IFileUploadService, WebFileUploadService>();

var app = builder.Build();

// Auto-migrate database on startup
using (var scope = app.Services.CreateScope())
{
    try 
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();

        // -------------------------------------------------------------
        // Auto-add new columns to avoid dropping database or using DB migrations 
        // -------------------------------------------------------------
        var isSqlite = db.Database.IsSqlite();
        var isSqlServer = db.Database.IsSqlServer();
        
        try
        {
            if (isSqlite) db.Database.ExecuteSqlRaw("ALTER TABLE Users ADD COLUMN VodafoneCashNumber TEXT;");
            else if (isSqlServer) db.Database.ExecuteSqlRaw("ALTER TABLE Users ADD VodafoneCashNumber NVARCHAR(MAX);");
        } catch { }

        try
        {
            if (isSqlite) db.Database.ExecuteSqlRaw("ALTER TABLE Users ADD COLUMN InstaPayId TEXT;");
            else if (isSqlServer) db.Database.ExecuteSqlRaw("ALTER TABLE Users ADD InstaPayId NVARCHAR(MAX);");
        } catch { }
        // -------------------------------------------------------------

        // ---- Seed Default Admin ----
        var adminEmail = "admin@sales.com";
        if (!db.Users.Any(u => u.Role == Sales.Shared.Models.UserRole.Admin))
        {
            var existingUser = db.Users.FirstOrDefault(u => u.Email == adminEmail);
            if (existingUser != null)
            {
                existingUser.Role = UserRole.Admin;
            }
            else
            {
                var admin = new AppUser
                {
                    FullName = "مدير النظام",
                    Email = adminEmail,
                    PasswordHash = "8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918", // admin
                    Role = UserRole.Admin,
                    CreatedAt = DateTime.UtcNow
                };
                db.Users.Add(admin);
            }
            db.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] Database Initialization Failed: {ex.Message}");
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
app.MapControllers(); 
app.UseStaticFiles();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(Sales.Shared._Imports).Assembly);

app.Run();
