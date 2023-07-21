using Microsoft.AspNetCore.Mvc;
using IdentityService.Auth;
using IdentityService.Endpoints.Helpers;

namespace IdentityService.Endpoints;

public class AuthEndpoint : IEndpoint
{
    public void DefineEndpoint(WebApplication app)
    {
        var group = app.MapGroup("api/auth/")
            .AllowAnonymous()
            .WithTags("Auth Endpoint");

        group.MapPost("login", Login)
            .WithName("Login");
        group.MapPost("register", Register)
            .WithName("Register");
    }

    internal async Task<IResult> Login(
        IAuthService IdentityService, 
        [FromBody] LoginUser.Request request,
        CancellationToken cancellationToken)
    {
        var validator = new LoginUser.RequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(
                new Result<LoginUser.Response>(ErrorType.Validation,
                    validationResult.Errors.Select(x => x.ErrorMessage)));
        }

        var result = await IdentityService.LoginAsync(request);
            
        return EndpointHelpers.MapResultToHttpResponse(result);
    }
    internal async Task<IResult> Register(
        IAuthService IdentityService,
        [FromBody] CreateUser.Request request,
        CancellationToken cancellationToken)
    {
        var validator = new CreateUser.RequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var ErrorResponse = new Result<LoginUser.Response>(
                ErrorType.Validation,
                validationResult.Errors.Select(x => x.ErrorMessage));
            return EndpointHelpers.MapResultToHttpResponse(ErrorResponse);
        }

        var result = await IdentityService.RegisterAsync(request);

        if (result.IsSuccess)
        {
            return Results.Ok();
        }
        return EndpointHelpers.MapResultToHttpResponse(result);
    }
}
