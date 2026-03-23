using Microsoft.AspNetCore.Mvc;
using Sales.Shared.Models;
using Sales.Shared.Services;

namespace Sales.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IDataService _data;

    public OrdersController(IDataService data)
    {
        _data = data;
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<Order>>> GetUserOrders(int userId)
    {
        return await _data.GetUserOrdersAsync(userId);
    }

    [HttpGet("merchant/{merchantId}")]
    public async Task<ActionResult<IEnumerable<Order>>> GetMerchantOrders(int merchantId)
    {
        return await _data.GetMerchantOrdersAsync(merchantId);
    }

    [HttpPost]
    public async Task<ActionResult> CreateOrder(Order order)
    {
        var (success, message, orderId) = await _data.CreateOrderAsync(order);
        return Ok(new { Success = success, Message = message, OrderId = orderId });
    }

    [HttpPut("{orderId}/status")]
    public async Task<ActionResult> UpdateStatus(int orderId, [FromBody] OrderStatus status)
    {
        var success = await _data.UpdateOrderStatusAsync(orderId, status);
        return success ? Ok() : NotFound();
    }

    [HttpGet("merchant/{merchantId}/count")]
    public async Task<ActionResult<int>> GetMerchantOrdersCount(int merchantId)
    {
        return await _data.GetMerchantOrdersCountAsync(merchantId);
    }

    [HttpGet("merchant/{merchantId}/revenue")]
    public async Task<ActionResult<decimal>> GetMerchantTotalRevenue(int merchantId)
    {
        return await _data.GetMerchantTotalRevenueAsync(merchantId);
    }
}
