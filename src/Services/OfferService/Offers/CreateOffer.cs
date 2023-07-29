using FluentValidation;

namespace OfferService.Offers;

public static class CreateOffer
{
    public record Request(
        string Title,
        string Category,
        string Description,
        decimal Price);

    internal class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .Length(5, 50);
            RuleFor(x => x.Description)
                .NotEmpty()
                .Length(20, 1000);
            RuleFor(x => x.Price)
                .NotEmpty()
                .GreaterThan(0);
        }
    }

    public record Response(Ulid Id);
}
