namespace IdentityService.Auth;

public interface IAuthService
{
    Task<Result<LoginUser.Response>> LoginAsync(LoginUser.Request request);
    Task<Result<bool?>> RegisterAsync(CreateUser.Request request);
    Task<bool> ValidateTokenAsync(string token);
}
