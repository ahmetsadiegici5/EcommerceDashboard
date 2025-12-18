using System.Security.Claims;
using System.Text.Encodings.Web;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EcommerceAPI.Authentication
{
    public class FirebaseAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
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

        public FirebaseAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var path = Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

            // Public endpoint'lerde authentication zorunlu değil
            if (IsPublicPath(path))
            {
                return AuthenticateResult.NoResult();
            }

            string? token = null;
            var authHeader = Request.Headers["Authorization"].ToString();

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = authHeader.Substring("Bearer ".Length);
            }
            else if (Request.Cookies.TryGetValue(SessionCookieName, out var cookieToken))
            {
                token = cookieToken;
            }

            if (string.IsNullOrEmpty(token))
            {
                return AuthenticateResult.Fail("Kimlik doğrulama token'ı bulunamadı.");
            }

            try
            {
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(token);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, decodedToken.Uid),
                    new Claim("user_id", decodedToken.Uid),
                    new Claim("uid", decodedToken.Uid)
                };

                foreach (var kvp in decodedToken.Claims)
                {
                    var value = kvp.Value?.ToString() ?? string.Empty;
                    claims.Add(new Claim(kvp.Key, value));

                    if (string.Equals(kvp.Key, ClaimTypes.Role, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(kvp.Key, "role", StringComparison.OrdinalIgnoreCase))
                    {
                        claims.Add(new Claim(ClaimTypes.Role, value));
                    }
                }

                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return AuthenticateResult.Success(ticket);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Firebase token doğrulaması başarısız.");
                return AuthenticateResult.Fail("Token doğrulanamadı.");
            }
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = 401;
            return Response.WriteAsJsonAsync(new
            {
                message = "Unauthorized",
                error = "Kimlik doğrulama gerekli."
            });
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
}


