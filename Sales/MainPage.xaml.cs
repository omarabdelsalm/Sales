using Sales.Shared.Data;

namespace Sales;

public partial class MainPage : ContentPage
{
    private readonly AppDbContext _db;

    public MainPage(AppDbContext db)
    {
        InitializeComponent();
        _db = db;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        try 
        {
            // التأكد من وجود قاعدة البيانات محلياً
            _db.Database.EnsureCreated();
            SeedAdmin();
        }
        catch (Exception ex)
        {
            // إذا كان هناك خطأ في هيكل البيانات (مثل نقص أعمدة)، نقوم بإعادة إنشائها
            // هذا التوجه مناسب لبيئة التطوير لضمان تزامن الهيكل مع الكود
            if (ex.Message.Contains("no such column") || (ex.InnerException?.Message.Contains("no such column") ?? false))
            {
                try 
                {
                    _db.Database.EnsureDeleted();
                    _db.Database.EnsureCreated();
                    SeedAdmin();
                }
                catch (Exception reEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Error recreating database: {reEx.Message}");
                }
            }
            else 
            {
                System.Diagnostics.Debug.WriteLine($"Error during database init: {ex.Message}");
            }
        }
    }

    private void SeedAdmin()
    {
        var adminEmail = "admin@sales.com";
        // استخدام Any() للتحقق من وجود أي مدير
        if (!_db.Users.Any(u => u.Role == Shared.Models.UserRole.Admin))
        {
            var existingUser = _db.Users.FirstOrDefault(u => u.Email == adminEmail);
            if (existingUser != null)
            {
                existingUser.Role = Shared.Models.UserRole.Admin;
            }
            else
            {
                var admin = new Shared.Models.AppUser
                {
                    FullName = "مدير النظام",
                    Email = adminEmail,
                    PasswordHash = "8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918", // admin
                    Role = Shared.Models.UserRole.Admin,
                    CreatedAt = DateTime.UtcNow
                };
                _db.Users.Add(admin);
            }
            _db.SaveChanges();
        }
    }
}
