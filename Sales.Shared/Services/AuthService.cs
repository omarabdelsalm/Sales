using Sales.Shared.Models;

namespace Sales.Shared.Services;

public class AuthService
{
    private readonly IDataService _data;
    public AppUser? CurrentUser { get; set; } // Set is now public for API responses to populate it
    public event Action? OnAuthChanged;

    public AuthService(IDataService data)
    {
        _data = data;
    }

    // ---- تسجيل مستخدم جديد ----
    public async Task<(bool Success, string Message)> RegisterAsync(
        string fullName, string email, string password, UserRole role, string? storeName = null)
    {
        return await _data.RegisterAsync(fullName, email, password, role, storeName);
    }

    // ---- تسجيل الدخول ----
    public async Task<(bool Success, string Message)> LoginAsync(string email, string password)
    {
        var user = await _data.LoginAsync(email, password);

        if (user is null)
            return (false, "البريد الإلكتروني أو كلمة المرور غير صحيحة.");

        CurrentUser = user;
        OnAuthChanged?.Invoke();
        return (true, "تم تسجيل الدخول بنجاح.");
    }

    // ---- تسجيل الخروج ----
    public void Logout()
    {
        CurrentUser = null;
        OnAuthChanged?.Invoke();
    }

    // ---- حذف الحساب ----
    public async Task<(bool Success, string Message)> DeleteAccountAsync()
    {
        if (CurrentUser == null) return (false, "لم يتم تسجيل الدخول.");

        var result = await _data.DeleteAccountAsync(CurrentUser.Id);
        if (result.Success)
        {
            Logout();
        }

        return result;
    }

    public bool IsLoggedIn => CurrentUser is not null;
    public bool IsMerchant => CurrentUser?.Role == UserRole.Merchant;
    public bool IsCustomer  => CurrentUser?.Role == UserRole.Customer;
    public bool IsAdmin     => CurrentUser?.Role == UserRole.Admin;

    // هل الاشتراك منتهي؟ (للتجار فقط)
    public bool IsSubscriptionExpired => IsMerchant && CurrentUser?.SubscriptionEndDate < DateTime.UtcNow;
}
