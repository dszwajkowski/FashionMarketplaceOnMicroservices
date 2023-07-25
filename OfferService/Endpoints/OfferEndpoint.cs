using Mapster;
using Microsoft.AspNetCore.Mvc;
using OfferService.Endpoints.Filters;
using OfferService.Models;
using OfferService.Offers;
using static OfferService.Endpoints.Helpers.EndpointHelpers;

namespace OfferService.Endpoints;

public class OfferEndpoint : IEndpoint
{
    public void DefineEndpoint(WebApplication app)
    {
        var group = app.MapGroup("offer");
        group.MapGet("/{id}", GetById);
        group.MapGet("", GetWithFilters);
        group.MapPost("", Create)
            .AddEndpointFilter<TokenValidationFilter>();
        group.MapPut("", Update)
            .AddEndpointFilter<TokenValidationFilter>();
    }

    internal async Task<IResult> GetById(IOfferRepository offerRepository, [FromRoute] string id, CancellationToken cancellationToken)
    {
        var validator = new GetOffer.RequestValidator();
        var validationResult = await validator.ValidateAsync(new GetOffer.Request(id), cancellationToken);
        if (!validationResult.IsValid)
        {
            return MapToHttpResponse(
                new Result<GetOffer.Response>(ErrorType.Validation, validationResult.Errors.Select(x => x.ErrorMessage)));
        }

        var result = await offerRepository.GetOfferByIdAsync(Ulid.Parse(id), cancellationToken);

        return MapToHttpResponse(result);
    }

    internal async Task<IResult> GetWithFilters(
        IOfferRepository offerRepository,
        GetFilteredOffers.Request request,
        CancellationToken cancellationToken)
    {
        var validator = new GetFilteredOffers.RequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return MapToHttpResponse(
                new Result<GetFilteredOffers.Response>(ErrorType.Validation,
                    validationResult.Errors.Select(x => x.ErrorMessage)));
        }

        var filters = request.Adapt<OfferFilters>();
        var offers = offerRepository.GetFilteredOffers(filters);
        var offersDto = offers.Adapt<List<GetFilteredOffers.OfferDto>>();
        return MapToHttpResponse(new Result<GetFilteredOffers.Response>(new GetFilteredOffers.Response(offersDto)));
    }

    internal async Task<IResult> Create(
        IOfferRepository offerRepository, 
        HttpContext context,
        [FromBody] CreateOffer.Request request, 
        CancellationToken cancellationToken)
    {
        var validator = new CreateOffer.RequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return MapToHttpResponse(
                new Result<CreateOffer.Response>(ErrorType.Validation, validationResult.Errors.Select(x => x.ErrorMessage)));
        }

        var offer = request.Adapt<Offer>();
        var result = await offerRepository.CreateOfferAsync(offer, cancellationToken);
        if (!await offerRepository.SaveChangesAsync())
        {
            return Results.Problem();
        }

        return MapToHttpResponse(new Result<CreateOffer.Response>(new CreateOffer.Response(result)));
    }

    internal async Task<IResult> Update(
        IOfferRepository offerRepository, 
        [FromBody] UpdateOffer.Request request,
        CancellationToken cancellationToken)
    {
        var validator = new UpdateOffer.RequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return MapToHttpResponse(
                new Result<bool>(ErrorType.Validation, validationResult.Errors.Select(x => x.ErrorMessage)));
        }

        var offer = request.Adapt<Offer>();
        var result = await offerRepository.UpdateOfferAsync(offer, cancellationToken);
        
        if (!result.IsSuccess)
        {
            return MapToHttpResponse(result);
        }

        if (!await offerRepository.SaveChangesAsync())
        {
            return Results.Problem();
        }

        return Results.Ok();
    }
}
