using FluentValidation;

namespace OfferService.Offers;

public static class GetFilteredOffers
{
    public record Request(
        string? Category,
        string? Title,
        string? Description,
        decimal? PriceMin,
        decimal? PriceMax,
        DateTime? DateAfter,
        DateTime? DateBefore,
        int? Page,
        int? PageSize)
    {


        public static ValueTask<Request> BindAsync(HttpContext context)
        {
            const string categoryKey = "category";
            const string titleKey = "title";
            const string descriptionKey = "description";
            const string priceMinKey = "pricemin";
            const string priceMaxKey = "pricemax";
            const string dateAfterKey = "dateafter";
            const string dateBeforeKey = "datebefore";
            const string pageKey = "page";
            const string pageSizeKey = "pagesize";

            decimal priceMin;
            bool priceMinParseRes = decimal.TryParse(context.Request.Query[priceMinKey], out priceMin);
            decimal priceMax;
            bool priceMaxParseRes = decimal.TryParse(context.Request.Query[priceMaxKey], out priceMax);
            DateTime dateAfter;
            bool pdateAfterParseRes = DateTime.TryParse(context.Request.Query[dateAfterKey], out dateAfter);
            DateTime dateBefore;
            bool dateBeforeParseRes = DateTime.TryParse(context.Request.Query[dateBeforeKey], out dateBefore);
            int page;
            bool pageParseRes = int.TryParse(context.Request.Query[pageKey], out page);
            int pageSize;
            bool pageSizeParseRes = int.TryParse(context.Request.Query[pageSizeKey], out pageSize);

            var request = new Request(
                context.Request.Query[categoryKey],
                context.Request.Query[titleKey],
                context.Request.Query[descriptionKey],
                priceMinParseRes ? priceMin : null,
                priceMaxParseRes ? priceMax : null,
                pdateAfterParseRes ? dateAfter : null,
                dateBeforeParseRes ? dateBefore : null,
                pageParseRes ? page : null,
                pageSizeParseRes ? pageSize : null
                );

            return ValueTask.FromResult(request);
        }
    };

    internal class RequestValidator : AbstractValidator<Request>
    {
        const int MaxPageSize = 100;

        public RequestValidator()
        {
            /*RuleFor(x => x.PageSize)
                .LessThanOrEqualTo(MaxPageSize);
            RuleFor(x => x.PriceMin)
                .GreaterThanOrEqualTo(0); // todo test null
            RuleFor(x => x.PriceMax)
                .GreaterThanOrEqualTo(0);*/
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
