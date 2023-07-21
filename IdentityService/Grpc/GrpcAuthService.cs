using Grpc.Core;
using IdentityService.Auth;

namespace IdentityService.Grpc;

public class GrpcAuthService : GrpcAuth.GrpcAuthBase
{
    private readonly IAuthService _authService;

    public GrpcAuthService(IAuthService authService)
    {
        _authService = authService;
    }

    public override async Task<TokenValidationResponse> ValidateToken(ValidateTokenRequest request, ServerCallContext context)
    {
        var response = new TokenValidationResponse();
        request.Token = request.Token.Replace("bearer ", "");
        var validationResult = await _authService.ValidateTokenAsync(request.Token);
        response.IsValid = validationResult;
        return response;
    }
}
