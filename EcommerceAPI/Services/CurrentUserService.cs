using System.Security.Claims;
using EcommerceAPI.Configuration;
using Microsoft.Extensions.Options;

namespace EcommerceAPI.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly SellerSettings _sellerSettings;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor, IOptions<SellerSettings> sellerOptions)
        {
            _httpContextAccessor = httpContextAccessor;
            _sellerSettings = sellerOptions.Value;
        }

        public string? GetUserId()
        {
            if (_sellerSettings.UseSharedSeller && !string.IsNullOrWhiteSpace(_sellerSettings.SharedSellerId))
            {
                return _sellerSettings.SharedSellerId;
            }

            return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                   ?? _httpContextAccessor.HttpContext?.User?.FindFirst("user_id")?.Value
                   ?? _httpContextAccessor.HttpContext?.User?.FindFirst("uid")?.Value;
        }

        public string GetUserIdOrThrow()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Kullanıcı kimliği doğrulanamadı.");
            }
            return userId;
        }
    }
}
