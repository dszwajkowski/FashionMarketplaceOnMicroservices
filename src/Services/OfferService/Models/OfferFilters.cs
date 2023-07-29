namespace OfferService.Models;

public class OfferFilters
{
    public Ulid? CursorBefore { get; set; }
    public Ulid? CursorAfter { get; set; }
    public int PageSize { get; set; }
    public string? Category { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public decimal? PriceMin { get; set; }
    public decimal? PriceMax { get; set; }
    public DateTime? DateAfter { get; set; }
    public DateTime? DateBefore { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
}
