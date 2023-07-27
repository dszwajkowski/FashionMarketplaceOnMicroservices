namespace IdentityService.Endpoints;

public class ProtoEndpoint : IEndpoint
{
    public void DefineEndpoint(WebApplication app)
    {
        var group = app.MapGroup("protos/")
            .RequireAuthorization();
        group.MapGet("auth.proto", async ctx =>
        {
            await ctx.Response.WriteAsync(File.ReadAllText("Grpc/auth.proto"));
        });
    }
}
