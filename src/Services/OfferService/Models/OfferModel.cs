using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OfferService.Models;

public class OfferModel
{
    [Key]
    [MaxLength(128)]
    public Guid Id { get; set; }
    [ForeignKey(nameof(Offer))]
    [MaxLength(128)]
    public Ulid OfferId { get; set; }
    [Required]
    [MaxLength(512)]
    public string Location { get; set; } = null!;
    [Required]
    [MaxLength(256)]
    public string FileName { get; set; } = null!;

    public virtual Offer Offer { get; set; } = null!;

    [NotMapped]
    public string FullPath => Path.Combine(Location, FileName);
}
