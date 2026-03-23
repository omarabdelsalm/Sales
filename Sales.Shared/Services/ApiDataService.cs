using System.Net.Http.Json;
using Sales.Shared.Models;

namespace Sales.Shared.Services;

public class ApiDataService : IDataService
{
    private readonly HttpClient _http;

    public ApiDataService(HttpClient http)
    {
        _http = http;
    }

    // ---- Auth ----
    public async Task<AppUser?> LoginAsync(string email, string password)
    {
        var response = await _http.PostAsJsonAsync("api/auth/login", new { Email = email, Password = password });
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<AppUser>();
        }
        return null;
    }

    public async Task<(bool Success, string Message)> RegisterAsync(string fullName, string email, string password, UserRole role, string? storeName = null)
    {
        var response = await _http.PostAsJsonAsync("api/auth/register", new { FullName = fullName, Email = email, Password = password, Role = role, StoreName = storeName });
        var result = await response.Content.ReadFromJsonAsync<AuthResult>();
        return (result?.Success ?? false, result?.Message ?? "خطأ في الاتصال بالسيرفر");
    }

    public async Task<(bool Success, string Message)> DeleteAccountAsync(int userId)
    {
        var response = await _http.DeleteAsync($"api/auth/account/{userId}");
        var result = await response.Content.ReadFromJsonAsync<AuthResult>();
        return (result?.Success ?? false, result?.Message ?? "خطأ في الاتصال بالسيرفر");
    }

    public async Task<bool> UpdateSubscriptionAsync(int userId, int months)
    {
        var response = await _http.PostAsJsonAsync($"api/auth/subscribe/{userId}", new { Months = months });
        return response.IsSuccessStatusCode;
    }

    // ---- Products ----
    public async Task<List<Product>> GetProductsAsync()
    {
        return await _http.GetFromJsonAsync<List<Product>>("api/products") ?? new();
    }

    public async Task<List<Product>> GetMerchantProductsAsync(int merchantId)
    {
        return await _http.GetFromJsonAsync<List<Product>>($"api/products/merchant/{merchantId}") ?? new();
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        return await _http.GetFromJsonAsync<Product>($"api/products/{id}");
    }

    public async Task<(bool Success, string Message)> SaveProductAsync(Product product)
    {
        HttpResponseMessage response;
        if (product.Id == 0)
            response = await _http.PostAsJsonAsync("api/products", product);
        else
            response = await _http.PutAsJsonAsync($"api/products/{product.Id}", product);

        if (response.IsSuccessStatusCode)
            return (true, "تم الحفظ بنجاح");
        
        return (false, "فشل الحفظ");
    }

    public async Task<(bool Success, string Message)> DeleteProductAsync(int id, int merchantId)
    {
        var response = await _http.DeleteAsync($"api/products/{id}?merchantId={merchantId}");
        if (response.IsSuccessStatusCode)
            return (true, "تم الحذف بنجاح");
        
        return (false, "فشل الحذف");
    }

    // ---- Orders ----
    public async Task<List<Order>> GetUserOrdersAsync(int userId)
    {
        return await _http.GetFromJsonAsync<List<Order>>($"api/orders/user/{userId}") ?? new();
    }

    public async Task<List<Order>> GetMerchantOrdersAsync(int merchantId)
    {
        return await _http.GetFromJsonAsync<List<Order>>($"api/orders/merchant/{merchantId}") ?? new();
    }

    public async Task<(bool Success, string Message, int OrderId)> CreateOrderAsync(Order order)
    {
        var response = await _http.PostAsJsonAsync("api/orders", order);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<OrderResult>();
            return (true, "تم الطلب بنجاح", result?.OrderId ?? 0);
        }
        return (false, "فشل إنشاء الطلب", 0);
    }

    public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status)
    {
        var response = await _http.PutAsJsonAsync($"api/orders/{orderId}/status", status);
        return response.IsSuccessStatusCode;
    }

    public async Task<int> GetMerchantOrdersCountAsync(int merchantId)
    {
        return await _http.GetFromJsonAsync<int>($"api/orders/merchant/{merchantId}/count");
    }

    public async Task<decimal> GetMerchantTotalRevenueAsync(int merchantId)
    {
        return await _http.GetFromJsonAsync<decimal>($"api/orders/merchant/{merchantId}/revenue");
    }

    // ---- Cart ----
    public async Task<List<CartItem>> GetCartItemsAsync(int userId)
    {
        return await _http.GetFromJsonAsync<List<CartItem>>($"api/cart/{userId}") ?? new();
    }

    public async Task<int> GetCartCountAsync(int userId)
    {
        return await _http.GetFromJsonAsync<int>($"api/cart/{userId}/count");
    }

    public async Task<(bool Success, string Message)> AddToCartAsync(int userId, int productId, int quantity)
    {
        var response = await _http.PostAsJsonAsync($"api/cart/{userId}/add", new { ProductId = productId, Quantity = quantity });
        if (response.IsSuccessStatusCode)
            return (true, "تمت الإضافة للسلة");
        return (false, "فشل الإضافة للسلة");
    }

    public async Task<bool> RemoveFromCartAsync(int userId, int productId)
    {
        var response = await _http.DeleteAsync($"api/cart/{userId}/{productId}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ClearCartAsync(int userId)
    {
        var response = await _http.DeleteAsync($"api/cart/{userId}");
        return response.IsSuccessStatusCode;
    }

    // ---- Admin ----
    public async Task<List<AppUser>> GetMerchantsAsync()
    {
        return await _http.GetFromJsonAsync<List<AppUser>>("api/admin/merchants") ?? new();
    }

    public async Task<bool> UpdateSubscriptionAsync(int userId, DateTime? endDate)
    {
        var response = await _http.PutAsJsonAsync($"api/admin/merchants/{userId}/subscription", endDate);
        return response.IsSuccessStatusCode;
    }

    private class AuthResult { public bool Success { get; set; } public string Message { get; set; } = ""; }
    private class OrderResult { public int OrderId { get; set; } }
}
