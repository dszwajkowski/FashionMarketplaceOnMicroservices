using FluentAssertions;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using OfferService.Endpoints;
using OfferService.Models;
using OfferService.Offers;
using static OfferService.Endpoints.Helpers.EndpointHelpers;

namespace OfferService.UnitTests;

public class EndpointTests
{
    private readonly OfferEndpoint _offerEndpoint;
    private readonly Mock<IOfferRepository> _offerRepositoryMock;

    public EndpointTests()
    {
        _offerEndpoint = new Endpoints.OfferEndpoint();
        _offerRepositoryMock = new Mock<IOfferRepository>();
    }

    [Fact]
    public async Task GetById_ReturnOffer_WhenOfferExists()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var fakeOffer = GetFakeOffer();
        _offerRepositoryMock.Setup(x => x.GetOfferByIdAsync(fakeOffer.Id, cancellationToken))
            .ReturnsAsync(new Result<Offer>(fakeOffer));
        var mappedFakeOffer = fakeOffer.Adapt<GetOffer.Response>();

        // Act
        var result = await _offerEndpoint.GetById(_offerRepositoryMock.Object, fakeOffer.Id.ToString(), cancellationToken) as Ok<GetOffer.Response>;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);
        result.Value.Should().BeEquivalentTo(mappedFakeOffer);
    }

    [Fact]
    public async Task GetById_ReturnNotFoundError_WhenOfferDoesntExists()
    {
        // Arrange
        var id = Ulid.NewUlid();
        var cancellationToken = CancellationToken.None;
        var errorResult = new Result<Offer>(ErrorType.NotFound, $"Offer with id {id} doesn't exists.");
        _offerRepositoryMock.Setup(x => x.GetOfferByIdAsync(id, cancellationToken))
            .ReturnsAsync(errorResult);

        // Act
        var result = await _offerEndpoint.GetById(_offerRepositoryMock.Object, id.ToString(), cancellationToken) as NotFound<HttpErrorBody>;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(404);
        result.Value.Should().NotBeNull();
        result.Value!.ErrorType.Should().Be(ErrorType.NotFound.ToString());
    }

    [Fact]
    public async Task GetById_ReturnValidationError_RequestNotValid()
    {
        // Arrange
        var id = "";
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _offerEndpoint.GetById(_offerRepositoryMock.Object, id, cancellationToken) as BadRequest<HttpErrorBody>;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(400);
        result.Value.Should().NotBeNull();
        result.Value!.ErrorType.Should().Be(ErrorType.Validation.ToString());
    }

    [Fact]
    public async Task GetWithFilters_ReturnOffers_WhenValidFilters()
    {
        // Arrange
        var request = new GetFilteredOffers.Request(null, null, null, null, null, null, null, null, null, null, null, null);
        var cancellationToken = CancellationToken.None;
        _offerRepositoryMock.Setup(x => x.GetFilteredOffers(It.IsAny<OfferFilters>()))
            .Returns(GetFakeOffers());

        // Act
        var result = await _offerEndpoint.GetWithFilters(_offerRepositoryMock.Object, request, cancellationToken) as Ok<GetFilteredOffers.Response>;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);
        result.Value.Should().NotBeNull();
        result.Value!.Offers.Count().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetWithFilters_ReturnValidationError_WhenRequestNotValid()
    {
        // Arrange
        var request = new GetFilteredOffers.Request(null, null, null, null, null, null, null, null, null, null, "notsortablefield", null);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _offerEndpoint.GetWithFilters(_offerRepositoryMock.Object, request, cancellationToken) as BadRequest<HttpErrorBody>;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(400);
        result.Value.Should().NotBeNull();
        result.Value!.ErrorType.Should().Be(ErrorType.Validation.ToString());
    }

    [Fact]
    public async Task CreateOffer_ReturnNewOfferId_OfferCreated()
    {
        // Arrange
        var request = new CreateOffer.Request("test offer", "T-Shirts", Guid.NewGuid(), "testing test offer 123", 20);
        var cancellationToken = CancellationToken.None;
        var id = Ulid.NewUlid();
        _offerRepositoryMock.Setup(x => x.CreateOfferAsync(It.IsAny<Offer>(), cancellationToken))
            .ReturnsAsync(id);
        _offerRepositoryMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(true);

        // Act
        var result = await _offerEndpoint.Create(_offerRepositoryMock.Object, request, cancellationToken) as Ok<CreateOffer.Response>;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(id);
    }

    [Fact]
    public async Task CreateOffer_ReturnValidationError_WhenRequestNotValid()
    {
        // Arrange
        var request = new CreateOffer.Request("", "", Guid.NewGuid(), "", 0);

        // Act
        var result = await _offerEndpoint.Create(_offerRepositoryMock.Object, request, CancellationToken.None) as BadRequest<HttpErrorBody>;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(400);
        result.Value.Should().NotBeNull();
        result.Value!.ErrorType.Should().Be(ErrorType.Validation.ToString());
    }

    [Fact]
    public async Task CreateOffer_ReturnProblem_WhenOfferNotSaved()
    {
        // Arrange
        var request = new CreateOffer.Request("test offer", "T-Shirts", Guid.NewGuid(), "testing test offer 123", 20);
        var cancellationToken = CancellationToken.None;
        _offerRepositoryMock.Setup(x => x.CreateOfferAsync(It.IsAny<Offer>(), cancellationToken))
            .ReturnsAsync(Ulid.NewUlid());
        _offerRepositoryMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(false);

        // Act
        var result = await _offerEndpoint.Create(_offerRepositoryMock.Object, request, cancellationToken) as ProblemHttpResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task UpdateOffer_ReturnOk_WhenOfferUpdated()
    {
        // Arrange
        var request = new UpdateOffer.Request(Ulid.NewUlid().ToString(), "T-Shirts", "testtitle", "test description of testtile offer", 2, true);
        var cancellationToken = CancellationToken.None;
        _offerRepositoryMock.Setup(x => x.UpdateOfferAsync(It.IsAny<Offer>(), cancellationToken))
            .ReturnsAsync(new Result<bool?>(null));
        _offerRepositoryMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(true);

        // Act
        var result = await _offerEndpoint.Update(_offerRepositoryMock.Object, request, cancellationToken) as Ok;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task UpdateOffer_ReturnValidationError_WhenRequestNotValid()
    {
        // Arrange
        var request = new UpdateOffer.Request("", "", "", "", 0, false);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _offerEndpoint.Update(_offerRepositoryMock.Object, request, cancellationToken) as BadRequest<HttpErrorBody>;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(400);
        result.Value.Should().NotBeNull();
        result.Value!.ErrorType.Should().Be(ErrorType.Validation.ToString());
    }

    [Fact]
    public async Task UpdateOffer_ReturnProblem_WhenOfferNotSaved()
    {
        // Arrange
        var request = new UpdateOffer.Request(Ulid.NewUlid().ToString(), "T-Shirts", "testtitle", "test description of testtile offer", 2, true);
        var cancellationToken = CancellationToken.None;
        _offerRepositoryMock.Setup(x => x.UpdateOfferAsync(It.IsAny<Offer>(), cancellationToken))
            .ReturnsAsync(new Result<bool?>(null));
        _offerRepositoryMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(false);

        // Act
        var result = await _offerEndpoint.Update(_offerRepositoryMock.Object, request, cancellationToken) as ProblemHttpResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(500);
    }

    private Offer GetFakeOffer()
    {
        return new Offer()
        {
            Id = Ulid.NewUlid(),
            Title = "Black t-shirt",
            Description = "testdesc",
            Category = "T-Shirts",
            Price = 25
        };
    }

    private List<Offer> GetFakeOffers()
    {
        return new List<Offer>
        {
            new()
            {
                Id = Ulid.NewUlid(),
                Title = "White t-shirt",
                Description = "testdesc",
                Category = "T-Shirts",
                Price = 20
            },
            new()
            {
                Id = Ulid.NewUlid(),
                Title = "Black jacket",
                Description = "testdesc",
                Category = "Jackets",
                Price = 100
            },
            GetFakeOffer()
        };
    }
}
