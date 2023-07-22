using Microsoft.AspNetCore.Mvc;
using OfferService.Data;
using OfferService.Endpoints.Filters;
using OfferService.Endpoints.Helpers;
using OfferService.Models;
using OfferService.Offers;

namespace OfferService.Endpoints;

public class OfferEndpoint : IEndpoint
{
    public void DefineEndpoint(WebApplication app)
    {
        var group = app.MapGroup("offer");
        group.MapGet("/{id}", GetById);
        group.MapGet("/getfiltered", GetWithFilters)
            .Produces<GetFilteredOffers.Request>();
        //.Accepts<GetFilteredOffers.Request>("");
        group.MapPost("", Create)
            .AddEndpointFilter<TokenValidationFilter>();
        group.MapPut("", Update)
            .AddEndpointFilter<TokenValidationFilter>();
        group.MapPut("/archive", Archive)
            .AddEndpointFilter<TokenValidationFilter>();
    }

    internal IResult GetById(ApplicationDbContext dbContext, [FromRoute] string id)
    {
        var offer = dbContext
            .Offers
            .Where(x => x.Id == Ulid.Parse(id))
            .SingleOrDefault(); // todo fix

        if (offer is null)
        {
            return EndpointHelpers.MapToHttpResponse(
                new Result<GetOffer.Response>(ErrorType.NotFound, $"Offer with id {id} doesn't exists."));
        }

        return EndpointHelpers.MapToHttpResponse(new Result<GetOffer.Response>(new GetOffer.Response()));
    }

    internal IResult GetWithFilters(ApplicationDbContext dbContext, GetFilteredOffers.Request request)
    {
        var filteredQuery = BuildFilters(dbContext, request);

        // todo map

        return EndpointHelpers.MapToHttpResponse(new Result<List<Offer>>(filteredQuery.ToList())); 
    }

    internal IResult Create(ApplicationDbContext dbContext, [FromBody] CreateOffer.Request request)
    {
        // todo map, extract creater from request
        var offer = new Offer
        {
            Title = request.Title,
            Category = request.Category,
            Description = request.Description,
            Price = request.Price
        };

        dbContext.Add(offer);
        dbContext.SaveChanges();

        return Results.Ok(new CreateOffer.Response(offer.Id));
    }

    internal IResult Update(ApplicationDbContext dbContext, [FromBody] UpdateOffer.Request request)
    {
        var offer = dbContext.Offers
            .Where(x => x.Id == Ulid.Parse(request.Id))
            .SingleOrDefault();

        if (offer is null)
        {
            return EndpointHelpers.MapToHttpResponse(
                new Result<bool>(ErrorType.NotFound, $"Offer with id {request.Id} doesn't exists."));
        }

        // todo map and save

        return Results.Ok();
    }

    internal IResult Archive(ApplicationDbContext dbContext)
    {
        // todo implementation
        return Results.Ok();
    }

    private IQueryable<Offer> BuildFilters(ApplicationDbContext dbContext, GetFilteredOffers.Request request)
    {
        var query = dbContext.Offers.AsQueryable();

        if (request.Category is not null) query = query.Where(x => x.Category == request.Category);
        if (request.Title is not null) query = query.Where(x => x.Title.Contains(request.Title));
        if (request.Description is not null) query = query.Where(x => x.Description.Contains(request.Description));
        if (request.PriceMin is not null) query = query.Where(x => x.Price >= request.PriceMin);
        if (request.PriceMax is not null) query = query.Where(x => x.Price <= request.PriceMax);
        if (request.DateAfter is not null) query = query.Where(x => x.DateAdded >= request.DateAfter);
        if (request.DateBefore is not null) query = query.Where(x => x.DateAdded <= request.DateBefore);

        return query;
    }
}
