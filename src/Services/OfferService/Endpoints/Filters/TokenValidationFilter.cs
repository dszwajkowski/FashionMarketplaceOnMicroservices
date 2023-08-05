using OfferService.Grpc;

namespace OfferService.Endpoints.Filters;

public static class TokenValidator
{
    public static void AddTokenValidator(this RouteHandlerBuilder builder)
    {
        builder.AddEndpointFilter<TokenValidationFilter>();
    }
}

public class TokenValidationFilter : IEndpointFilter
{
    private readonly IGrpcIdentityService _identityService;

    public TokenValidationFilter(IGrpcIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        const string headerName = "Authorization";
        if (!context.HttpContext.Request.Headers.TryGetValue(headerName, out var tokenHeader))
        {
            return Results.Unauthorized();
        }

        string token = tokenHeader.ToString();
        token = token.Replace("bearer ", "", StringComparison.OrdinalIgnoreCase);
        
        if (string.IsNullOrEmpty(token)) 
        {
            return Results.Unauthorized();
        }

        var validationResult = await _identityService.ValidateToken(token);
        if (!validationResult)
        {
            return Results.Unauthorized();
        }

        return await next(context);
    }
}
