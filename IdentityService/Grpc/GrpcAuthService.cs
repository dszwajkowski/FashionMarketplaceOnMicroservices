using Grpc.Core;
using IdentityService.Auth;

namespace IdentityService.Grpc;

public class GrpcAuthService : GrpcAuth.GrpcAuthBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<GrpcAuthService> _logger;

    public GrpcAuthService(IAuthService authService, ILogger<GrpcAuthService> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public override async Task<TokenValidationResponse> ValidateToken(ValidateTokenRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Token validation request received from service {Service}.", request.ServiceName);

        var response = new TokenValidationResponse();
        request.Token = request.Token.Replace("bearer ", "");
        var validationResult = await _authService.ValidateTokenAsync(request.Token);
        response.IsValid = validationResult;

        if (!response.IsValid) 
        {
            _logger.LogWarning("Service {Service} didn't provide valid token: {Token}.", request.ServiceName, request.Token);
        }

        return response;
    }
}
