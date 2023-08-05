namespace OrderService.Endpoints.Helpers;

internal static class EndpointHelpers
{
    internal static IResult MapToHttpResponse<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(result.Data);
        }

        return result.ErrorType switch
        {
            ErrorType.Validation => Results.BadRequest(new HttpErrorBody(
                result.ErrorType.ToString()!,
                result.ErrorMessages!)),
            ErrorType.NotFound => Results.NotFound(new HttpErrorBody(
                result.ErrorType.ToString()!,
                result.ErrorMessages!)),
            ErrorType.Unauthorized => Results.Unauthorized(),
            ErrorType.Forbidden => Results.Forbid(),
            _ => TypedResults.BadRequest(),
        };
    }

    internal record HttpErrorBody(
        string ErrorType,
        IEnumerable<string> ErrorMessages);
}
