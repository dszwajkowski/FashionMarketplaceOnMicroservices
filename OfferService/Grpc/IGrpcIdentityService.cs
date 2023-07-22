namespace OfferService.Grpc;

public interface IGrpcIdentityService
{
    Task<bool> ValidateToken(string token);
}
