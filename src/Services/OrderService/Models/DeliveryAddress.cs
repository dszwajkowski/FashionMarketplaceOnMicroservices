namespace OrderService.Models;

public class DeliveryAddress
{
    public string Country { get; set; } = null!;
    public string City { get; set; } = null!;
    public string AddressLine { get; set; } = null!;
    public string PostalCode { get; set; } = null!;
}
