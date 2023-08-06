using System.ComponentModel.DataAnnotations;

namespace OrderService.Models;

public class PaymentMethod
{
    [Key]
    public int Id { get; set; }
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;
}

public enum PaymentMethods
{
    Online = 1,
    CashOnArrival = 2
}
