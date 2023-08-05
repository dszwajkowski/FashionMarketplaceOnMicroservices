namespace OrderService.Endpoints;

public class SwaggerEndpoint : IEndpoint
{
    public void DefineEndpoint(WebApplication app)
    {
        app.Map("/", () => Results.Redirect("/swagger"));
    }
}
