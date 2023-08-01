using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OfferService.Models;

public class Offer
{
    [Key]
    [MaxLength(128)]
    public Ulid Id { get; set; }
    [Required]
    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; }
    [Required]
    public string Category { get; set; } = null!;
    [Required]
    [MaxLength(50)]
    public string Title { get; set; } = null!;
    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = null!;
    [Required]
    public decimal Price { get; set; }
    [Required]
    public DateTime DateAdded { get; init; } = DateTime.UtcNow;
    public DateTime? DateModified { get; set; }
    [Required]
    public bool Active { get; set; } = true;
    [Required]
    public bool Visible { get; set; } = true;

    public virtual User User { get; set; } = null!;
    public virtual ICollection<OfferPhoto> Photos { get; set; } = null!;
}
