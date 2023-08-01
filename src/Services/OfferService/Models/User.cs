using System.ComponentModel.DataAnnotations;

namespace OfferService.Models;

public class User
{
    [Key]
    [Required]
    public Guid Id { get; set; }
    [Required]
    [MaxLength(15)]
    public string UserName { get; set; } = null!;
    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; } = null!;
    [Required]
    [MaxLength(50)]
    public string SecondName { get; set; } = null!;
    [Required]
    public string Email { get; set; } = null!;
    [MaxLength(15)]
    public string PhoneNumber { get; set; } = null!;
}
