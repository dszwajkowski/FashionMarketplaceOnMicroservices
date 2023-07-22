using OfferService.Grpc;

namespace OfferService.Endpoints.Middlewares;

public class TokenValidationMiddleware
{
    private readonly RequestDelegate _next;

    public TokenValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IGrpcIdentityService identityService)
    {
        const string headerName = "Authorization";
        if (!context.Request.Headers.TryGetValue(headerName, out var tokenHeader)
                || string.IsNullOrEmpty(tokenHeader))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Token not valid.");
            return;
        }

        string token = tokenHeader.ToString();
        token = token.Replace("bearer ", "", StringComparison.OrdinalIgnoreCase);

        try
        {
            var validationResult = await identityService.ValidateToken(token!);
            if (!validationResult)
            {
                await context.Response.WriteAsync("Token not valid.");
                return;
            }
        }
        catch (Exception) 
        {
            context.Response.StatusCode = 500;
            // todo log
        }

        await _next(context);
    }
}
