using System.ComponentModel.DataAnnotations;

namespace OfferService.Models;

public class Offer
{
    [Key]
    [MaxLength(128)]
    public Ulid Id { get; set; }
    [Required]
    public Guid CreatorId { get; set; }
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
    public bool Visible { get; set; }

    public virtual ICollection<OfferModel> Photos { get; set; } = null!;
}
