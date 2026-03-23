namespace Sales.Shared.Models;

public enum OrderStatus { Pending, Confirmed, Shipped, Delivered, Cancelled }
public enum PaymentMethod { CashOnDelivery, VodafoneCash, InstaPay }

public class Order
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;
    public string? PaymentReference { get; set; } // رقم العملية أو الملاحظات
    public string? Notes { get; set; }

    // Foreign Key
    public int UserId { get; set; }
    public AppUser? User { get; set; }

    // Navigation
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
