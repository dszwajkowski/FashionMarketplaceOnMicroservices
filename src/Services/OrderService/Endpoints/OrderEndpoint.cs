using EventBus;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Endpoints.Filters;
using OrderService.Features.Orders;
using OrderService.Models;
using System.IdentityModel.Tokens.Jwt;
using static OrderService.Endpoints.Helpers.EndpointHelpers;

namespace OrderService.Endpoints;

public class OrderEndpoint : IEndpoint
{
    public void DefineEndpoint(WebApplication app)
    {
        var group = app.MapGroup("order");
        group.MapPost("", Create)
            .AddTokenValidator();
        //group.MapGet("", GetUsersOrders)
       //     .AddTokenValidator();
        group.MapGet("{id}", GetById)
            .AddTokenValidator();
    }

    internal async Task<IResult> Create(
        ApplicationDbContext dbContext,
        HttpContext httpContext,
        IEventBus eventBus,
        [FromBody] CreateOrder.Request request, 
        CancellationToken cancellationToken)
    {
        // todo extract logic to diffrent methods

        var validator = new CreateOrder.RequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return MapToHttpResponse(
                new Result<CreateOrder.Response>(ErrorType.Validation, validationResult.Errors.Select(x => x.ErrorMessage)));
        }

        // todo fix enum mapping
        var order = request.Adapt<Order>();

        var sub = httpContext.User.Claims
            .FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub)?.Value;
        if (sub is not null)
        {
            order.UserId = Guid.Parse(sub);
        }
        else
        {
            return Results.Unauthorized();
        }

        // if buyer will pay on package arrival, approve order immediately
        if (order.PaymentMethodId == (int)PaymentMethods.CashOnArrival)
        {
            order.StatusId = (int)OrderStatuses.Approved;
        }
        else
        {
            order.StatusId = (int)OrderStatuses.WaitingForPayment;
        }

        await dbContext.Orders.AddAsync(order);
        
        if (await dbContext.SaveChangesAsync(cancellationToken) == 0) 
        {
            return Results.Problem();
        }

        return Results.Ok(new CreateOrder.Response(order.Id));
    }

    internal async Task<IResult> GetById(
        ApplicationDbContext dbContext,
        HttpContext httpContext,
        Guid id,
        CancellationToken cancellationToken)
    {
        var request = new GetOrder.Request(id);
        var validator = new GetOrder.RequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return MapToHttpResponse(
                new Result<GetOrder.Response>(ErrorType.Validation, validationResult.Errors.Select(x => x.ErrorMessage)));
        }

        var order = await dbContext.Orders
            .Where(x => x.Id == request.Id)
            .SingleOrDefaultAsync();

        if (order == null)
        {
            return MapToHttpResponse(new Result<GetOrder.Response>(ErrorType.NotFound, 
                $"Order id {request.Id} doesn't exists."));
        }
        
        // todo make method to extract user model from token
        var sub = httpContext.User.Claims
            .FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub)?.Value;
        // todo offer author and admin should be able to view order aswell
        if (sub == null || order.UserId != Guid.Parse(sub))
        {
            return Results.Unauthorized();
        }

        var orderResponse = order.Adapt<GetOrder.Response>();
        return Results.Ok(orderResponse);
    }

    internal async Task<IResult> GetUsersOrders(
        ApplicationDbContext dbContext,
        HttpContext httpContext,
        GetOrder.Request request,
        [FromBody]CancellationToken cancellationToken)
    {
        var validator = new GetOrder.RequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return MapToHttpResponse(
                new Result<GetOrder.Response>(ErrorType.Validation, validationResult.Errors.Select(x => x.ErrorMessage)));
        }

        return Results.Ok();
    }
}
