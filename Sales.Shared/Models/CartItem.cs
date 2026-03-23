namespace Sales.Shared.Models;

public class CartItem
{
    public int Id { get; set; }
    public int Quantity { get; set; } = 1;

    // Foreign Keys
    public int UserId { get; set; }
    public AppUser? User { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }
}
