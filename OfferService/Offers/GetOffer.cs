using FluentValidation;

namespace OfferService.Offers;

public static class GetOffer
{
    public record Request(string Id);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.Id)
                .Must(x => Ulid.TryParse(x, out _))
                .WithMessage("");
        }
    }

    public class Response
    {
        public Ulid Id { get; set; }
        public string Category { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime? DateModified { get; set; }
    }
}
