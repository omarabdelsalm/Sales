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

    public string BaseUrl => _http.BaseAddress?.ToString()?.TrimEnd('/') ?? "";

    private async Task<T?> SafeGetAsync<T>(string url)
    {
        try
        {
            return await _http.GetFromJsonAsync<T>(url);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API Error (GET {url}): {ex.Message}");
            return default;
        }
    }

    private async Task<HttpResponseMessage?> SafePostAsync<T>(string url, T data)
    {
        try
        {
            return await _http.PostAsJsonAsync(url, data);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API Error (POST {url}): {ex.Message}");
            return null;
        }
    }

    private async Task<HttpResponseMessage?> SafePutAsync<T>(string url, T data)
    {
        try
        {
            return await _http.PutAsJsonAsync(url, data);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API Error (PUT {url}): {ex.Message}");
            return null;
        }
    }

    private async Task<HttpResponseMessage?> SafeDeleteAsync(string url)
    {
        try
        {
            return await _http.DeleteAsync(url);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API Error (DELETE {url}): {ex.Message}");
            return null;
        }
    }

    // ---- Auth ----
    public async Task<AppUser?> LoginAsync(string email, string password)
    {
        var response = await SafePostAsync("api/auth/login", new { Email = email, Password = password });
        if (response?.IsSuccessStatusCode == true)
        {
            return await response.Content.ReadFromJsonAsync<AppUser>();
        }
        return null;
    }

    public async Task<(bool Success, string Message)> RegisterAsync(string fullName, string email, string password, UserRole role, string? storeName = null)
    {
        var response = await SafePostAsync("api/auth/register", new { FullName = fullName, Email = email, Password = password, Role = role, StoreName = storeName });
        if (response == null) return (false, "خطأ في الاتصال بالسيرفر");
        var result = await response.Content.ReadFromJsonAsync<AuthResult>();
        return (result?.Success ?? false, result?.Message ?? "خطأ في معالجة البيانات");
    }

    public async Task<(bool Success, string Message)> DeleteAccountAsync(int userId)
    {
        var response = await SafeDeleteAsync($"api/auth/account/{userId}");
        if (response == null) return (false, "خطأ في الاتصال بالسيرفر");
        var result = await response.Content.ReadFromJsonAsync<AuthResult>();
        return (result?.Success ?? false, result?.Message ?? "خطأ في معالجة البيانات");
    }

    public async Task<bool> UpdateSubscriptionAsync(int userId, int months)
    {
        var response = await SafePostAsync($"api/auth/subscribe/{userId}", new { Months = months });
        return response?.IsSuccessStatusCode == true;
    }

    public async Task<AppUser?> GetUserByIdAsync(int userId)
    {
        return await SafeGetAsync<AppUser>($"api/auth/user/{userId}");
    }

    public async Task<(bool Success, string Message)> UpdateMerchantPaymentDetailsAsync(int merchantId, string? vcash, string? instapay)
    {
        var response = await SafePutAsync($"api/auth/merchant/{merchantId}/payment-details", new { VodafoneCashNumber = vcash, InstaPayId = instapay });
        if (response == null) return (false, "خطأ في الاتصال بالسيرفر");
        var result = await response.Content.ReadFromJsonAsync<AuthResult>();
        return (result?.Success ?? false, result?.Message ?? "خطأ في معالجة البيانات");
    }

    // ---- Products ----
    public async Task<List<Product>> GetProductsAsync()
    {
        return await SafeGetAsync<List<Product>>("api/products") ?? new();
    }

    public async Task<List<Product>> GetMerchantProductsAsync(int merchantId)
    {
        return await SafeGetAsync<List<Product>>($"api/products/merchant/{merchantId}") ?? new();
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        return await SafeGetAsync<Product>($"api/products/{id}");
    }

    public async Task<(bool Success, string Message)> SaveProductAsync(Product product)
    {
        HttpResponseMessage? response;
        if (product.Id == 0)
            response = await SafePostAsync("api/products", product);
        else
            response = await SafePutAsync($"api/products/{product.Id}", product);

        if (response?.IsSuccessStatusCode == true)
            return (true, "تم الحفظ بنجاح");
        
        return (false, "فشل الحفظ أو الاتصال بالسيرفر");
    }

    public async Task<(bool Success, string Message)> DeleteProductAsync(int id, int merchantId)
    {
        var response = await SafeDeleteAsync($"api/products/{id}?merchantId={merchantId}");
        if (response?.IsSuccessStatusCode == true)
            return (true, "تم الحذف بنجاح");
        
        return (false, "فشل الحذف أو الاتصال بالسيرفر");
    }

    // ---- Orders ----
    public async Task<List<Order>> GetUserOrdersAsync(int userId)
    {
        return await SafeGetAsync<List<Order>>($"api/orders/user/{userId}") ?? new();
    }

    public async Task<List<Order>> GetMerchantOrdersAsync(int merchantId)
    {
        return await SafeGetAsync<List<Order>>($"api/orders/merchant/{merchantId}") ?? new();
    }

    public async Task<(bool Success, string Message, int OrderId)> CreateOrderAsync(Order order)
    {
        var response = await SafePostAsync("api/orders", order);
        if (response?.IsSuccessStatusCode == true)
        {
            var result = await response.Content.ReadFromJsonAsync<OrderResult>();
            return (true, "تم الطلب بنجاح", result?.OrderId ?? 0);
        }
        return (false, "فشل إنشاء الطلب", 0);
    }

    public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status)
    {
        var response = await SafePutAsync($"api/orders/{orderId}/status", status);
        return response?.IsSuccessStatusCode == true;
    }

    public async Task<int> GetMerchantOrdersCountAsync(int merchantId)
    {
        return await SafeGetAsync<int>($"api/orders/merchant/{merchantId}/count");
    }

    public async Task<decimal> GetMerchantTotalRevenueAsync(int merchantId)
    {
        return await SafeGetAsync<decimal>($"api/orders/merchant/{merchantId}/revenue");
    }

    // ---- Cart ----
    public async Task<List<CartItem>> GetCartItemsAsync(int userId)
    {
        return await SafeGetAsync<List<CartItem>>($"api/cart/{userId}") ?? new();
    }

    public async Task<int> GetCartCountAsync(int userId)
    {
        return await SafeGetAsync<int>($"api/cart/{userId}/count");
    }

    public async Task<(bool Success, string Message)> AddToCartAsync(int userId, int productId, int quantity)
    {
        var response = await SafePostAsync($"api/cart/{userId}/add", new { ProductId = productId, Quantity = quantity });
        if (response?.IsSuccessStatusCode == true)
            return (true, "تمت الإضافة للسلة");
        return (false, "فشل الإضافة للسلة");
    }

    public async Task<bool> RemoveFromCartAsync(int userId, int productId)
    {
        var response = await SafeDeleteAsync($"api/cart/{userId}/{productId}");
        return response?.IsSuccessStatusCode == true;
    }

    public async Task<bool> ClearCartAsync(int userId)
    {
        var response = await SafeDeleteAsync($"api/cart/{userId}");
        return response?.IsSuccessStatusCode == true;
    }

    // ---- Admin ----
    public async Task<List<AppUser>> GetMerchantsAsync()
    {
        return await SafeGetAsync<List<AppUser>>("api/admin/merchants") ?? new();
    }

    public async Task<bool> UpdateSubscriptionAsync(int userId, DateTime? endDate)
    {
        var response = await SafePutAsync($"api/admin/merchants/{userId}/subscription", endDate);
        return response?.IsSuccessStatusCode == true;
    }

    private class AuthResult { public bool Success { get; set; } public string Message { get; set; } = ""; }
    private class OrderResult { public int OrderId { get; set; } }
}
