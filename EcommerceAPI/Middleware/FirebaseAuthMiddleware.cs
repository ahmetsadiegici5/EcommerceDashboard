using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace EcommerceAPI.Middleware
{
    public class FirebaseAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<FirebaseAuthMiddleware> _logger;
        private const string SessionCookieName = "authToken";

        // Kimlik doğrulama gerektirmeyen endpoint'ler
        private static readonly string[] PublicPaths = new[]
        {
            "/api/auth/register",
            "/api/auth/session",
            "/api/auth/verify-token",
            "/swagger",
            "/health"
        };

        public FirebaseAuthMiddleware(RequestDelegate next, ILogger<FirebaseAuthMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";

            // Public endpoint'lere erişime izin ver
            if (IsPublicPath(path))
            {
                await _next(context);
                return;
            }

            string? token = null;
            string? authHeader = context.Request.Headers["Authorization"].ToString();

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                token = authHeader.Substring("Bearer ".Length);
            }
            else if (context.Request.Cookies.TryGetValue(SessionCookieName, out var cookieToken))
            {
                token = cookieToken;
            }

            // Token yoksa ve public path değilse 401 döndür
            if (string.IsNullOrEmpty(token))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { message = "Unauthorized", error = "Kimlik doğrulama gerekli." });
                return;
            }

            try
            {
                FirebaseToken decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(token!);
                
                // Create ClaimsPrincipal
                var claims = new List<System.Security.Claims.Claim>
                {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, decodedToken.Uid),
                    new System.Security.Claims.Claim("user_id", decodedToken.Uid),
                    new System.Security.Claims.Claim("uid", decodedToken.Uid)
                };

                foreach (var claim in decodedToken.Claims)
                {
                    claims.Add(new System.Security.Claims.Claim(claim.Key, claim.Value.ToString() ?? ""));
                }

                var identity = new System.Security.Claims.ClaimsIdentity(claims, "Firebase");
                context.User = new System.Security.Claims.ClaimsPrincipal(identity);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Firebase kimlik doğrulaması başarısız. TokenPrefix={TokenPrefix}",
                    token!.Substring(0, Math.Min(token.Length, 20)));

                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { message = "Unauthorized", error = ex.Message });
                return;
            }

            await _next(context);
        }

        private static bool IsPublicPath(string path)
        {
            foreach (var publicPath in PublicPaths)
            {
                if (path.StartsWith(publicPath, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }

    // Extension method to use the middleware
    public static class FirebaseAuthMiddlewareExtensions
    {
        public static IApplicationBuilder UseFirebaseAuth(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<FirebaseAuthMiddleware>();
        }
    }
}