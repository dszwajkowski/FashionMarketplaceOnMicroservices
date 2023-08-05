using Microsoft.AspNetCore.Mvc;
using OrderService.Endpoints.Filters;
using OrderService.Features.Orders;
using static OrderService.Endpoints.Helpers.EndpointHelpers;

namespace OrderService.Endpoints;

public class OrderEndpoint : IEndpoint
{
    public void DefineEndpoint(WebApplication app)
    {
        var group = app.MapGroup("order");
        group.MapPost("", Create)
            .AddTokenValidator();
    }

    internal async Task<IResult> Create(
        [FromBody] CreateOrder.Request request, 
        CancellationToken cancellationToken)
    {
        var validator = new CreateOrder.RequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return MapToHttpResponse(
                new Result<CreateOrder.Response>(ErrorType.Validation, validationResult.Errors.Select(x => x.ErrorMessage)));
        }

        return Results.Ok();
    }
}
