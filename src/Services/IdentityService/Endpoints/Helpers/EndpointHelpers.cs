using IdentityService.Auth;

namespace IdentityService.Endpoints.Helpers;

internal static class EndpointHelpers
{
    internal static IResult MapResultToHttpResponse<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(result.Data);
        }

        return result.ErrorType switch
        {
            ErrorType.Validation => Results.BadRequest(new
            {
                ErrorType = result.ErrorType.ToString(),
                result.ErrorMessages
            }),
            ErrorType.NotFound => Results.NotFound(new
            {
                ErrorType = result.ErrorType.ToString(),
                result.ErrorMessages
            }),
            ErrorType.Unauthorized => Results.Unauthorized(),
            ErrorType.Forbidden => Results.Forbid(),
            _ => Results.BadRequest(),
        };
    }
}
