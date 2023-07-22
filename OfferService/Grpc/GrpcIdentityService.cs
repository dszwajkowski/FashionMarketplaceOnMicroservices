using Grpc.Net.Client;

namespace OfferService.Grpc;

public class GrpcIdentityService : IGrpcIdentityService
{
    private readonly IConfiguration _configuration;

    public GrpcIdentityService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Validates token in IdentityService. Throws exception if connection fails.
    /// </summary>
    /// <param name="token">JWT token</param>
    /// <returns></returns>
    public Task<bool> ValidateToken(string token)
    {
        var channel = GrpcChannel.ForAddress(_configuration["GrpcIdentityService"]!);
        var client = new GrpcAuth.GrpcAuthClient(channel);
        var request = new ValidateTokenRequest()
        {
            Token = token
        };

        TokenValidationResponse? response = null;
        try
        {
            response = client.ValidateToken(request);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not call Grpc Server: {ex.Message}"); //todo log
            throw;
        }
        return Task.FromResult(response!.IsValid);
    }
}
