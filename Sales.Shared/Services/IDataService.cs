using Sales.Shared.Models;

namespace Sales.Shared.Services;

public interface IDataService
{
    string BaseUrl { get; }

    // ---- Auth ----
    Task<AppUser?> LoginAsync(string email, string password);
    Task<(bool Success, string Message)> RegisterAsync(string fullName, string email, string password, UserRole role, string? storeName = null);
    Task<(bool Success, string Message)> DeleteAccountAsync(int userId);
    Task<bool> UpdateSubscriptionAsync(int userId, int months);

    // ---- Products ----
    Task<List<Product>> GetProductsAsync();
    Task<List<Product>> GetMerchantProductsAsync(int merchantId);
    Task<Product?> GetProductByIdAsync(int id);
    Task<(bool Success, string Message)> SaveProductAsync(Product product);
    Task<(bool Success, string Message)> DeleteProductAsync(int id, int merchantId);

    // ---- Orders ----
    Task<List<Order>> GetUserOrdersAsync(int userId);
    Task<List<Order>> GetMerchantOrdersAsync(int merchantId);
    Task<(bool Success, string Message, int OrderId)> CreateOrderAsync(Order order);
    Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status);
    Task<int> GetMerchantOrdersCountAsync(int merchantId);
    Task<decimal> GetMerchantTotalRevenueAsync(int merchantId);

    // ---- Cart ----
    Task<List<CartItem>> GetCartItemsAsync(int userId);
    Task<int> GetCartCountAsync(int userId);
    Task<(bool Success, string Message)> AddToCartAsync(int userId, int productId, int quantity);
    Task<bool> RemoveFromCartAsync(int userId, int productId);
    Task<bool> ClearCartAsync(int userId);

    // ---- Admin ----
    Task<List<AppUser>> GetMerchantsAsync();
    Task<bool> UpdateSubscriptionAsync(int userId, DateTime? endDate);
}
