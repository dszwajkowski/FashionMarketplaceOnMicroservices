namespace IdentityService.Endpoints;

public class TestEndpoint : IEndpoint
{
    public void DefineEndpoint(WebApplication app)
    {
        app.MapGet("test2", test)
            .RequireAuthorization();
    }

    internal IResult test()
    {

        return Results.Ok();
    }
}
