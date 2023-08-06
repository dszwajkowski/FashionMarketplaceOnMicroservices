using FluentValidation;
using OrderService.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace OrderService.Features.Orders;

public static class CreateOrder
{
    public record Request
    {
        public string OfferId { get; init; } = null!;
        public DeliveryAddress DeliveryAddress { get; init; } = null!;
        public string PaymentMethod { get; init; } = null!;
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
            RuleFor(x => x.PaymentMethod).IsEnumName(typeof(PaymentMethods), false);
            // todo validate if userid is valid ulid
        }
    }

    public record Response(Guid Id);

    public record OrderCreatedEvent
    {
        public Guid Id { get; init; }
        public string OfferId { get; init; } = null!;
        public Guid UserId { get; init; }
        public DeliveryAddress DeliveryAddress { get; init; } = null!;
        public DateTime CreatedDate { get; private init; }
        public string Status { get; init; } = null!;
        public string PaymentMethod { get; init; } = null!;
    }
}
