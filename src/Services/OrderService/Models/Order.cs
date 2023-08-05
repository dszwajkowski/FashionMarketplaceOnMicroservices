namespace OrderService.Models;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OfferId { get; set; } = null!;
    public Guid UserId { get; set; }
    public DeliveryAddress DeliveryAddress { get; set; } = null!;
    public DateTime CreatedDate { get; private set; } = DateTime.UtcNow;
    public PaymentMethod PaymentMethod { get; set; }
    public DateTime? PaymentDate { get; set; }
}
