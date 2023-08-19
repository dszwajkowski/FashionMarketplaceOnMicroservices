using FluentValidation;
using OrderService.Models;

namespace OrderService.Features.Orders;

public static class GetOrders
{
    public record Request(Guid UserId);

    internal class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
        }
    }

    public record Response
    {
        public Guid Id { get; init; }
        public string OfferId { get; init; } = null!;
        public Guid UserId { get; init; }
        public DeliveryAddress DeliveryAddress { get; init; } = null!;
        public DateTime CreatedDate { get; init; }
        public string PaymentMethod { get; init; } = null!;
        public DateTime? PaymentDate { get; init; }
    }
}



