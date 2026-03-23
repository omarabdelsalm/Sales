using Microsoft.EntityFrameworkCore;
using Sales.Shared.Data;
using Sales.Shared.Models;
using System.Security.Cryptography;
using System.Text;

namespace Sales.Shared.Services;

public class DbDataService : IDataService
{
    private readonly AppDbContext _db;

    public DbDataService(AppDbContext db)
    {
        _db = db;
    }

    // ---- Auth ----
    public async Task<AppUser?> LoginAsync(string email, string password)
    {
        email = email.Trim().ToLowerInvariant();
        var hash = HashPassword(password);
        return await _db.Users
            .FirstOrDefaultAsync(u => u.Email == email && u.PasswordHash == hash && !u.IsDeleted);
    }

    public async Task<(bool Success, string Message)> RegisterAsync(string fullName, string email, string password, UserRole role, string? storeName = null)
    {
        email = email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(u => u.Email == email))
            return (false, "البريد الإلكتروني مستخدم بالفعل.");

        var user = new AppUser
        {
            FullName = fullName.Trim(),
            Email = email,
            PasswordHash = HashPassword(password),
            Role = role,
            StoreName = role == UserRole.Merchant ? storeName : null,
            CreatedAt = DateTime.UtcNow,
            SubscriptionEndDate = role == UserRole.Merchant ? DateTime.UtcNow.AddMonths(2) : null
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return (true, "تم التسجيل بنجاح.");
    }

    public async Task<(bool Success, string Message)> DeleteAccountAsync(int userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return (false, "المستخدم غير موجود.");

        user.IsDeleted = true;
        user.Email = $"deleted_{user.Id}_{user.Email}";
        user.FullName = "Deleted User";
        user.PasswordHash = "";
        user.PhoneNumber = null;
        user.StoreName = null;
        user.SubscriptionEndDate = null;

        await _db.SaveChangesAsync();
        return (true, "تم حذف الحساب بنجاح.");
    }

    public async Task<bool> UpdateSubscriptionAsync(int userId, int months)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return false;

        user.SubscriptionEndDate = (user.SubscriptionEndDate ?? DateTime.UtcNow).AddMonths(months);
        await _db.SaveChangesAsync();
        return true;
    }

    // ---- Products ----
    public async Task<List<Product>> GetProductsAsync()
    {
        return await _db.Products
            .Include(p => p.Merchant)
            .Where(p => p.IsAvailable && p.Merchant != null && !p.Merchant.IsDeleted)
            .Where(p => p.Merchant!.Role != UserRole.Merchant || p.Merchant.SubscriptionEndDate > DateTime.UtcNow)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Product>> GetMerchantProductsAsync(int merchantId)
    {
        return await _db.Products
            .Where(p => p.MerchantId == merchantId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        return await _db.Products
            .Include(p => p.Merchant)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<(bool Success, string Message)> SaveProductAsync(Product product)
    {
        if (product.Id == 0)
        {
            _db.Products.Add(product);
        }
        else
        {
            var existing = await _db.Products.FindAsync(product.Id);
            if (existing == null) return (false, "المنتج غير موجود.");
            
            _db.Entry(existing).CurrentValues.SetValues(product);
        }
        
        await _db.SaveChangesAsync();
        return (true, "تم حفظ المنتج بنجاح.");
    }

    public async Task<(bool Success, string Message)> DeleteProductAsync(int id, int merchantId)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return (false, "المنتج غير موجود.");
        if (product.MerchantId != merchantId) return (false, "غير مصرح لك بحذف هذا المنتج.");

        _db.Products.Remove(product);
        await _db.SaveChangesAsync();
        return (true, "تم حذف المنتج بنجاح.");
    }

    // ---- Orders ----
    public async Task<List<Order>> GetUserOrdersAsync(int userId)
    {
        return await _db.Orders
            .Include(o => o.Items)
            .ThenInclude(oi => oi.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<List<Order>> GetMerchantOrdersAsync(int merchantId)
    {
        return await _db.Orders
            .Include(o => o.Items)
            .ThenInclude(oi => oi.Product)
            .Where(o => o.Items.Any(oi => oi.Product != null && oi.Product.MerchantId == merchantId))
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<(bool Success, string Message, int OrderId)> CreateOrderAsync(Order order)
    {
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();
        return (true, "تم إنشاء الطلب بنجاح.", order.Id);
    }

    public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status)
    {
        var order = await _db.Orders.FindAsync(orderId);
        if (order == null) return false;
        order.Status = status;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetMerchantOrdersCountAsync(int merchantId)
    {
        return await _db.OrderItems
            .Where(oi => oi.Product != null && oi.Product.MerchantId == merchantId)
            .Select(oi => oi.OrderId)
            .Distinct()
            .CountAsync();
    }

    public async Task<decimal> GetMerchantTotalRevenueAsync(int merchantId)
    {
        return await _db.OrderItems
            .Where(oi => oi.Product != null && oi.Product.MerchantId == merchantId)
            .SumAsync(oi => (decimal?)oi.UnitPrice * oi.Quantity) ?? 0;
    }

    // ---- Cart ----
    public async Task<List<CartItem>> GetCartItemsAsync(int userId)
    {
        return await _db.CartItems
            .Include(c => c.Product)
            .Where(c => c.UserId == userId)
            .ToListAsync();
    }

    public async Task<int> GetCartCountAsync(int userId)
    {
        return await _db.CartItems
            .Where(c => c.UserId == userId)
            .SumAsync(c => c.Quantity);
    }

    public async Task<(bool Success, string Message)> AddToCartAsync(int userId, int productId, int quantity)
    {
        var existing = await _db.CartItems
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);

        if (existing != null)
        {
            existing.Quantity += quantity;
        }
        else
        {
            _db.CartItems.Add(new CartItem { UserId = userId, ProductId = productId, Quantity = quantity });
        }

        await _db.SaveChangesAsync();
        return (true, "تمت الإضافة للسلة");
    }

    public async Task<bool> RemoveFromCartAsync(int userId, int productId)
    {
        var item = await _db.CartItems.FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);
        if (item == null) return false;

        _db.CartItems.Remove(item);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ClearCartAsync(int userId)
    {
        var items = await _db.CartItems.Where(c => c.UserId == userId).ToListAsync();
        _db.CartItems.RemoveRange(items);
        await _db.SaveChangesAsync();
        return true;
    }

    // ---- Admin ----
    public async Task<List<AppUser>> GetMerchantsAsync()
    {
        return await _db.Users
            .Where(u => u.Role == UserRole.Merchant && !u.IsDeleted)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> UpdateSubscriptionAsync(int userId, DateTime? endDate)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return false;
        user.SubscriptionEndDate = endDate;
        await _db.SaveChangesAsync();
        return true;
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
