using Microsoft.AspNetCore.Mvc;
using Sales.Shared.Models;
using Sales.Shared.Services;

namespace Sales.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly IDataService _data;

    public CartController(IDataService data)
    {
        _data = data;
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<IEnumerable<CartItem>>> GetCartItems(int userId)
    {
        return await _data.GetCartItemsAsync(userId);
    }

    [HttpGet("{userId}/count")]
    public async Task<ActionResult<int>> GetCartCount(int userId)
    {
        return await _data.GetCartCountAsync(userId);
    }

    [HttpPost("{userId}/add")]
    public async Task<ActionResult> AddToCart(int userId, [FromBody] AddToCartRequest request)
    {
        var (success, message) = await _data.AddToCartAsync(userId, request.ProductId, request.Quantity);
        return success ? Ok(new { Success = true, Message = message }) : BadRequest(new { Success = false, Message = message });
    }

    [HttpDelete("{userId}/{productId}")]
    public async Task<ActionResult> RemoveFromCart(int userId, int productId)
    {
        var success = await _data.RemoveFromCartAsync(userId, productId);
        return success ? Ok() : NotFound();
    }

    [HttpDelete("{userId}")]
    public async Task<ActionResult> ClearCart(int userId)
    {
        var success = await _data.ClearCartAsync(userId);
        return success ? Ok() : BadRequest();
    }

    public class AddToCartRequest { public int ProductId { get; set; } public int Quantity { get; set; } }
}
