using OfferService.Models;

namespace OfferService.Offers;

public interface IOfferRepository
{
    Task<Result<Offer>> GetOfferByIdAsync(Ulid id, CancellationToken cancellationToken);
    IEnumerable<Offer> GetFilteredOffers(OfferFilters filters);
    Task<Ulid> CreateOfferAsync(Offer offer, CancellationToken cancellationToken);
    Task<Result<bool?>> UpdateOfferAsync(Offer offer, CancellationToken cancellationToken);
    Task<bool> SaveChangesAsync();
}
