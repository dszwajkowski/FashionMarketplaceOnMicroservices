using FluentValidation;

namespace OfferService.Offers;

public static class UpdateOffer
{
    public record Request(
        string Id,
        string Title,
        string Description,
        decimal Price);

    internal class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .Must(x => Ulid.TryParse(x, out _));
            RuleFor(x => x.Title)
                .NotEmpty()
                .Length(5, 50);
            RuleFor(x => x.Description)
                .NotEmpty()
                .Length(20, 1000);
        }
    }
}
