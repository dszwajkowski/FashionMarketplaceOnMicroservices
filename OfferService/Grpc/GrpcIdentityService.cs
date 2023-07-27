using Grpc.Net.Client;

namespace OfferService.Grpc;

public class GrpcIdentityService : IGrpcIdentityService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GrpcIdentityService> _logger;

    public GrpcIdentityService(IConfiguration configuration, ILogger<GrpcIdentityService> logger)
    {
        _configuration = configuration;
        _logger = logger;
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
            Token = token,
            ServiceName = "OfferService"
        };

        TokenValidationResponse? response = null;
        try
        {
            response = client.ValidateToken(request);
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Couldn't validate token, gRPC connection to IdentityService failed.");
            throw;
        }
        return Task.FromResult(response!.IsValid);
    }
}
