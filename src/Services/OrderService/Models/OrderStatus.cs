using System.ComponentModel.DataAnnotations;

namespace OrderService.Models;

public class OrderStatus
{
    [Key]
    public int Id { get; set; }
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;
}


public enum OrderStatuses
{
    WaitingForPayment = 1,
    Approved = 2,
    ToBeShipped = 3,
    Shipped = 4,
    Delivered = 5,
    Closed = 6,
    Cancelled = 7
}
