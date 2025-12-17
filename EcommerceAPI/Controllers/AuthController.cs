using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using EcommerceAPI.Services;
using EcommerceAPI.DTOs;

namespace EcommerceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly FirebaseAuthService _authService;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<AuthController> _logger;
        private const string SessionCookieName = "authToken";

        public AuthController(
            FirebaseAuthService authService,
            IWebHostEnvironment environment,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _environment = environment;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            try
            {
                var user = await _authService.CreateUserAsync(model.Email, model.Password);
                await _authService.CreateTokenAsync(user.Uid); // Set default claims

                return Ok(new
                {
                    message = "Kullanıcı oluşturuldu. Lütfen Firebase kimlik bilgilerinizi kullanarak giriş yapın.",
                    user = new
                    {
                        Id = user.Uid,
                        Email = user.Email,
                        Role = "seller"
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("verify-token")]
        public async Task<IActionResult> VerifyToken([FromBody] VerifyTokenModel model)
        {
            try
            {
                var decodedToken = await _authService.VerifyTokenAsync(model.Token);
                return Ok(new { 
                    Valid = true,
                    Uid = decodedToken.Uid,
                    Claims = decodedToken.Claims
                });
            }
            catch
            {
                return Ok(new { Valid = false });
            }
        }

        [HttpPost("session")]
        public async Task<IActionResult> CreateSession([FromBody] SessionRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.IdToken))
                return BadRequest("Geçerli bir Firebase ID token gerekli.");

            try
            {
                var decodedToken = await _authService.VerifyTokenAsync(request.IdToken);
                var expirationSeconds = decodedToken.ExpirationTimeSeconds;
                var expiresAt = expirationSeconds > 0
                    ? DateTimeOffset.FromUnixTimeSeconds(expirationSeconds)
                    : DateTimeOffset.UtcNow.AddHours(1);

                var cookieOptions = BuildSessionCookieOptions(expiresAt);
                Response.Cookies.Append(SessionCookieName, request.IdToken, cookieOptions);

                return Ok(new
                {
                    userId = decodedToken.Uid,
                    expiresAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Session oluşturulamadı: Token doğrulanamadı.");
                return Unauthorized(new { message = "Token doğrulanamadı." });
            }
        }

        [HttpDelete("session")]
        public IActionResult DeleteSession()
        {
            Response.Cookies.Delete(SessionCookieName, new CookieOptions
            {
                HttpOnly = true,
                Secure = !_environment.IsDevelopment(),
                SameSite = SameSiteMode.Lax,
                Path = "/"
            });

            return NoContent();
        }

        private CookieOptions BuildSessionCookieOptions(DateTimeOffset expiresAt)
        {
            return new CookieOptions
            {
                HttpOnly = true,
                Secure = !_environment.IsDevelopment(),
                SameSite = SameSiteMode.Lax,
                Expires = expiresAt,
                IsEssential = true,
                Path = "/"
            };
        }
    }
}