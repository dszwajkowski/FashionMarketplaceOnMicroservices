using Grpc.Core;
using Grpc.Net.Client;
using Polly;
using Polly.Contrib.WaitAndRetry;

namespace OfferService.Grpc;

public class GrpcIdentityService : IGrpcIdentityService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GrpcIdentityService> _logger;
    private readonly IAsyncPolicy<TokenValidationResponse> _retryPolicy =
        Policy<TokenValidationResponse>
            .Handle<RpcException>()
            .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 5));

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
    public async Task<bool> ValidateToken(string token)
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
            response = await _retryPolicy.ExecuteAsync(async () => await client.ValidateTokenAsync(request));
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Couldn't validate token, gRPC connection to IdentityService failed.");
            throw;
        }
        return response!.IsValid;
    }
}
