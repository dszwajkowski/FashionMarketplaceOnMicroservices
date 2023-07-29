namespace IdentityService.Auth;

public interface IAuthService
{
    Task<Result<LoginUser.Response>> LoginAsync(LoginUser.Request request, CancellationToken cancellationToken);
    Task<Result<bool?>> RegisterAsync(CreateUser.Request request, CancellationToken cancellationToken);
    Task<bool> ValidateTokenAsync(string token);
}
