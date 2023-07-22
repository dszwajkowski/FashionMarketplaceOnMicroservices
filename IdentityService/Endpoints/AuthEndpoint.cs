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
        var result = await IdentityService.LoginAsync(request, cancellationToken);
            
        return EndpointHelpers.MapResultToHttpResponse(result);
    }
    internal async Task<IResult> Register(
        IAuthService IdentityService,
        [FromBody] CreateUser.Request request,
        CancellationToken cancellationToken)
    {
        var result = await IdentityService.RegisterAsync(request, cancellationToken);

        return EndpointHelpers.MapResultToHttpResponse(result);
    }
}
