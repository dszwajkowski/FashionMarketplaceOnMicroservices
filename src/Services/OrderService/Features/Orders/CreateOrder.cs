using FluentValidation;
using OrderService.Models;

namespace OrderService.Features.Orders;

public static class CreateOrder
{
    public record Request
    {
        public string OfferId { get; set; } = null!;
        public Guid UserId { get; set; }
        public DeliveryAddress DeliveryAddress { get; set; } = null!;
        public string PaymentMethod { get; set; } = null!;
    }

    internal class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.OfferId).NotEmpty();
            RuleFor(x => x.DeliveryAddress).NotEmpty();
            RuleFor(x => x.DeliveryAddress.Country).NotEmpty();
            RuleFor(x => x.DeliveryAddress.City).NotEmpty();
            RuleFor(x => x.DeliveryAddress.AddressLine).NotEmpty();
            RuleFor(x => x.DeliveryAddress.PostalCode).NotEmpty();
            RuleFor(x => x.PaymentMethod).IsEnumName(typeof(PaymentMethod), false);
        }
    }

    public record Response(Guid Id);
}
