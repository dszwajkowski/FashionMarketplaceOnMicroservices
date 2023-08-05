namespace OrderService.Grpc;

public interface IGrpcIdentityService
{
    Task<bool> ValidateToken(string token);
}
