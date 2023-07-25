using FluentValidation;

namespace OfferService.Offers;

public static class GetFilteredOffers
{
    public record Request(
        Ulid? CursorBefore,
        Ulid? CursorAfter,
        int? PageSize,
        string? Category,
        string? Title,
        string? Description,
        decimal? PriceMin,
        decimal? PriceMax,
        DateTime? DateAfter,
        DateTime? DateBefore,
        string? SortBy,
        string? SortDirection)
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
            const string cursorBeforeKey = "cursorBefore";
            const string cursorAfterKey = "cursorAfter";
            const string pageSizeKey = "pagesize";
            const string sortFieldKey = "sortby";
            const string sortDirectionKey = "sortdir";

            decimal priceMin;
            bool priceMinParseRes = decimal.TryParse(context.Request.Query[priceMinKey], out priceMin);
            decimal priceMax;
            bool priceMaxParseRes = decimal.TryParse(context.Request.Query[priceMaxKey], out priceMax);
            DateTime dateAfter;
            bool pdateAfterParseRes = DateTime.TryParse(context.Request.Query[dateAfterKey], out dateAfter);
            DateTime dateBefore;
            bool dateBeforeParseRes = DateTime.TryParse(context.Request.Query[dateBeforeKey], out dateBefore);
            Ulid cursorBefore;
            bool cursorBeforeRes = Ulid.TryParse(context.Request.Query[cursorBeforeKey], out cursorBefore);
            Ulid cursorAfter;
            bool cursorAfterRes = Ulid.TryParse(context.Request.Query[cursorAfterKey], out cursorAfter);
            int pageSize;
            bool pageSizeParseRes = int.TryParse(context.Request.Query[pageSizeKey], out pageSize);

            var request = new Request(
                cursorBeforeRes ? cursorBefore : null,
                cursorAfterRes ? cursorAfter : null,
                pageSizeParseRes ? pageSize : null,
                context.Request.Query[categoryKey],
                context.Request.Query[titleKey],
                context.Request.Query[descriptionKey],
                priceMinParseRes ? priceMin : null,
                priceMaxParseRes ? priceMax : null,
                pdateAfterParseRes ? dateAfter : null,
                dateBeforeParseRes ? dateBefore : null,
                context.Request.Query[sortFieldKey],
                context.Request.Query[sortDirectionKey]
                );

            return ValueTask.FromResult(request);
        }
    };

    internal class RequestValidator : AbstractValidator<Request>
    {
        const int MaxPageSize = 100;
        readonly string[] sortableFields = new string[] { "dateadded", "price", "title" };
        readonly string[] sortDirections = new string[] { "asc", "desc" };

        public RequestValidator()
        { 
            RuleFor(x => x.PageSize)
                .LessThanOrEqualTo(MaxPageSize);
            RuleFor(x => x.SortBy)
                .NotEmpty()
                .When(x => !string.IsNullOrEmpty(x.SortDirection));
            RuleFor(x => x.SortBy)
                .Must(x => sortableFields.Contains(x))
                .When(x => !string.IsNullOrEmpty(x.SortBy))
                .WithMessage($"Allowed fields for sorting: {string.Join(", ", sortableFields)}");
            RuleFor(x => x.SortDirection)
                .Must(x => sortDirections.Contains(x))
                .When(x => !string.IsNullOrEmpty(x.SortDirection))
                .WithMessage($"Allowed sorting direction: {string.Join(", ", sortDirections)}");
        }
    } 

    public record OfferDto(
        Ulid Id,
        string Category,
        string Title,
        string Description,
        decimal Price,
        DateTime DateAdded,
        DateTime? DateModified);

    public record Response(IEnumerable<OfferDto> offers);
}
