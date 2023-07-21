using IdentityService.Endpoints;

namespace IdentityService.Configuration;

internal static class EndpointsConfiguration
{
    internal static void AddEndpoints(this WebApplication app)
    {
        var endpoints = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .Where(x => x.IsClass && typeof(IEndpoint).IsAssignableFrom(x));

        foreach (var endpoint in endpoints)
        {
            var instance = Activator.CreateInstance(endpoint) as IEndpoint;
            instance!.DefineEndpoint(app);
        }
    }
}
