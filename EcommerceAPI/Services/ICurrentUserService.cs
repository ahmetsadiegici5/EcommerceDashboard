namespace EcommerceAPI.Services
{
    public interface ICurrentUserService
    {
        string? GetUserId();
        string GetUserIdOrThrow();
    }
}
