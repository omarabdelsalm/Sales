namespace Sales.Shared.Models;

public enum UserRole { Customer, Merchant, Admin }

public class AppUser
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public UserRole Role { get; set; } = UserRole.Customer;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? PhoneNumber { get; set; }
    public string? StoreName { get; set; }  // للتجار فقط
    public DateTime? SubscriptionEndDate { get; set; } // تاريخ انتهاء الاشتراك أو التجربة
    public string? VodafoneCashNumber { get; set; }
    public string? InstaPayId { get; set; }
    public bool IsDeleted { get; set; } = false;

    // Navigation
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
