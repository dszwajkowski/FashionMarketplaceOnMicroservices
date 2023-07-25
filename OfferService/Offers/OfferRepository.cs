using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OfferService.Data;
using OfferService.Models;

namespace OfferService.Offers;

public class OfferRepository : IOfferRepository
{
    private readonly ApplicationDbContext _dbContext;

    public OfferRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Offer>> GetOfferByIdAsync(Ulid id, CancellationToken cancellationToken)
    {
        var offer = await _dbContext.Offers
            .Where(x => x.Id == id)
            .SingleOrDefaultAsync(cancellationToken);

        if (offer is null)
        {
            return new Result<Offer>(ErrorType.NotFound, $"Offer with id {id} doesn't exists.");
        }

        return new Result<Offer>(offer);
    }

    public IEnumerable<Offer> GetFilteredOffers(OfferFilters filters)
    {
        return BuildFilteredQuery(filters)
            .AsEnumerable();
    }

    public async Task<Ulid> CreateOfferAsync(Offer offer, CancellationToken cancellationToken)
    {
        offer.Id = Ulid.NewUlid();
        await _dbContext.Offers.AddAsync(offer, cancellationToken);
        return offer.Id;
    }

    public async Task<Result<bool?>> UpdateOfferAsync(Offer offer, CancellationToken cancellationToken)
    {
        var offerFromDb = await _dbContext.Offers.FindAsync(offer.Id);

        if (offerFromDb is null) 
        {
            return new Result<bool?>(ErrorType.NotFound, $"Offer with id {offer.Id} doesn't exists.");
        }

        _dbContext.Entry(offerFromDb).CurrentValues.SetValues(offer);

        return new Result<bool?>(null);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _dbContext.SaveChangesAsync() > 0;
    }

    private IQueryable<Offer> BuildFilteredQuery(OfferFilters filters)
    {
        var query = _dbContext.Offers.AsQueryable();

        if (filters.CursorAfter is not null) query = query.Where(x => x.Id.CompareTo(filters.CursorAfter.Value) > 0);
        if (filters.CursorBefore is not null) query = query.Where(x => x.Id.CompareTo(filters.CursorBefore.Value) < 0);
        if (filters.Category is not null) query = query.Where(x => x.Category == filters.Category);
        if (filters.Title is not null) query = query.Where(x => x.Title.Contains(filters.Title));
        if (filters.Description is not null) query = query.Where(x => x.Description.Contains(filters.Description));
        if (filters.PriceMin is not null) query = query.Where(x => x.Price >= filters.PriceMin);
        if (filters.PriceMax is not null) query = query.Where(x => x.Price <= filters.PriceMax);
        if (filters.DateAfter is not null) query = query.Where(x => x.DateAdded >= filters.DateAfter);
        if (filters.DateBefore is not null) query = query.Where(x => x.DateAdded <= filters.DateBefore);

        if (!string.IsNullOrEmpty(filters.SortBy))
        {
            const string descSortKey = "desc";
            const string dateAddedKey = "dateadded";
            const string priceKey = "price";
            const string titleKey = "title";
            if (filters.SortDirection == descSortKey)
            {
                query = filters.SortBy switch
                {
                    dateAddedKey => query.OrderByDescending(x => x.Id), // ulid is lexicographically sortable so we can sort by id
                    priceKey => query.OrderByDescending(x => x.Price),
                    titleKey => query.OrderByDescending(x => x.Title),
                    _ => query.OrderByDescending(x => x.Id)
                };
            }
            else
            {
                query = filters.SortBy switch
                {
                    dateAddedKey => query.OrderBy(x => x.Id), // ulid is lexicographically sortable so we can sort by id
                    priceKey => query.OrderBy(x => x.Price),
                    titleKey => query.OrderBy(x => x.Title),
                    _ => query.OrderBy(x => x.Id)
                };
            }
        }

        return query;
    }
}

