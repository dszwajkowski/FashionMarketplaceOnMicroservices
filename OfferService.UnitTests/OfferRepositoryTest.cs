using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OfferService.Data;
using OfferService.Models;
using OfferService.Offers;

namespace OfferService.UnitTests;

public class OfferRepositoryTest
{
    private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;
    private readonly Ulid _ulid = Ulid.Parse("01ARZ3NDEKTSV4RRFFQ69G5FAV");

    public OfferRepositoryTest()
    {
        _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
           .UseInMemoryDatabase(Guid.NewGuid().ToString())
           .Options;

        using var dbContext = new ApplicationDbContext(_dbContextOptions);
        dbContext.Offers.AddRange(GetFakeOffers());
        dbContext.SaveChanges();
    }

    [Fact]
    public async Task GetOfferById_ReturnOffer_WhenValidId()
    {
        // Arrange
        var dbContext = new ApplicationDbContext(_dbContextOptions);
        var offerRepository = new OfferRepository(dbContext);

        // Act
        var result = await offerRepository.GetOfferByIdAsync(_ulid, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(_ulid);
    }

    [Fact]
    public async Task GetOfferByIdAsync_ReturnError_WhenNotValidId()
    {
        // Arrange
        var dbContext = new ApplicationDbContext(_dbContextOptions);
        var offerRepository = new OfferRepository(dbContext);

        // Act
        var result = await offerRepository.GetOfferByIdAsync(Ulid.NewUlid(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
        result.ErrorMessages.Should().NotBeNullOrEmpty();
        result.ErrorMessages!.Count().Should().Be(1);
    }

    [Fact]
    public void GetFilteredOffers_ReturnOffers_WhenValidFilter()
    {
        // Arrange
        var dbContext = new ApplicationDbContext(_dbContextOptions);
        var offerRepository = new OfferRepository(dbContext);
        var filters = new OfferFilters { Category = "Jackets" };

        // Act
        var result = offerRepository.GetFilteredOffers(filters).ToList();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Count.Should().Be(1);
    }

    [Fact]
    public async Task CreateOfferASync_ReturnOffers_WhenValidFilter()
    {
        // Arrange
        var dbContext = new ApplicationDbContext(_dbContextOptions);
        var offerRepository = new OfferRepository(dbContext);
        var offer = new Offer { Id = Ulid.NewUlid(), Title = "Black t-shirt", Description = "testdesc", Category = "T-Shirts", Price = 20 };

        // Act
        var result = await offerRepository.CreateOfferAsync(offer,CancellationToken.None);

        // Assert
        result.Should().NotBe(Ulid.Empty);
    }

    [Fact]
    public async Task SaveChangesAsync_ReturnTrue_WhenSavedEntity()
    {
        // Arrange
        var dbContext = new ApplicationDbContext(_dbContextOptions);
        var offerRepository = new OfferRepository(dbContext);
        var offer = new Offer { Id = Ulid.NewUlid(), Title = "Black t-shirt", Description = "testdesc", Category = "T-Shirts", Price = 20 };
        
        // Act
        await offerRepository.CreateOfferAsync(offer, CancellationToken.None);
        var result = await offerRepository.SaveChangesAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SaveChangesAsync_ReturnFalse_WhenNoChanges()
    {
        // Arrange
        var dbContext = new ApplicationDbContext(_dbContextOptions);
        var offerRepository = new OfferRepository(dbContext);

        // Act
        var result = await offerRepository.SaveChangesAsync();

        // Assert
        result.Should().BeFalse();
    }

    private List<Offer> GetFakeOffers()
    {
        return new List<Offer>
        {
            new Offer { Id = _ulid, Title = "White t-shirt", Description = "testdesc", Category = "T-Shirts", Price = 20 },
            new Offer { Id = Ulid.NewUlid(), Title = "Black jacket", Description = "testdesc", Category = "Jackets", Price = 100 }
        };
    }

}