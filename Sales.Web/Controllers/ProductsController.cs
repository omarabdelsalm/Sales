using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sales.Shared.Data;
using Sales.Shared.Models;
using Sales.Shared.Services;

namespace Sales.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IDataService _data;

    public ProductsController(IDataService data)
    {
        _data = data;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
    {
        return await _data.GetProductsAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        var product = await _data.GetProductByIdAsync(id);
        if (product == null) return NotFound();
        return product;
    }

    [HttpGet("merchant/{merchantId}")]
    public async Task<ActionResult<IEnumerable<Product>>> GetMerchantProducts(int merchantId)
    {
        return await _data.GetMerchantProductsAsync(merchantId);
    }

    [HttpPost]
    public async Task<ActionResult<(bool Success, string Message)>> SaveProduct(Product product)
    {
        return await _data.SaveProductAsync(product);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<(bool Success, string Message)>> UpdateProduct(int id, Product product)
    {
        product.Id = id;
        return await _data.SaveProductAsync(product);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<(bool Success, string Message)>> DeleteProduct(int id, [FromQuery] int merchantId)
    {
        return await _data.DeleteProductAsync(id, merchantId);
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadImage([FromServices] IFileUploadService fileUpload)
    {
        try
        {
            var file = Request.Form.Files.FirstOrDefault();
            if (file == null) return BadRequest("لم يتم اختيار ملف");

            using var stream = file.OpenReadStream();
            var path = await fileUpload.SaveImageAsync(stream, file.FileName);
            
            return Ok(new { Path = path });
        }
        catch (Exception ex)
        {
            return BadRequest($"خطأ في رفع الملف: {ex.Message}");
        }
    }
}
