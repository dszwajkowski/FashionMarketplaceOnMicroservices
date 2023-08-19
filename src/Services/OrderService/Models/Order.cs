using Mapster;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderService.Models;

public class Order
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required]
    [MaxLength(128)]
    public string OfferId { get; set; } = null!;
    [Required]
    public Guid UserId { get; set; }
    public DeliveryAddress DeliveryAddress { get; set; } = null!;
    [Required]
    public DateTime CreatedDate { get; private set; } = DateTime.UtcNow;
    [Required]
    [ForeignKey(nameof(OrderStatus))]
    public int StatusId { get; set; }
    [Required]
    [ForeignKey(nameof(PaymentMethod))]
    public int PaymentMethodId { get; set; }
    public DateTime? PaymentDate { get; set; }

    [AdaptIgnore]
    public virtual OrderStatus Status { get; set; } = null!;
    [AdaptIgnore]
    public virtual PaymentMethod PaymentMethod { get; set; } = null!;
}
