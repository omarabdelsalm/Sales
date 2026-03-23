using Microsoft.AspNetCore.Mvc;
using Sales.Shared.Models;
using Sales.Shared.Services;

namespace Sales.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IDataService _data;

    public AdminController(IDataService data)
    {
        _data = data;
    }

    [HttpGet("merchants")]
    public async Task<ActionResult<IEnumerable<AppUser>>> GetMerchants()
    {
        return await _data.GetMerchantsAsync();
    }

    [HttpPut("merchants/{userId}/subscription")]
    public async Task<ActionResult> UpdateSubscription(int userId, [FromBody] DateTime? endDate)
    {
        var success = await _data.UpdateSubscriptionAsync(userId, endDate);
        return success ? Ok() : NotFound();
    }
}
