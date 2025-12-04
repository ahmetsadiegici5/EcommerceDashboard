using Microsoft.AspNetCore.Mvc;
using EcommerceAPI.Services;

namespace EcommerceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly FirebaseAuthService _authService;

        public AuthController(FirebaseAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            try
            {
                var user = await _authService.CreateUserAsync(model.Email, model.Password);
                var token = await _authService.CreateTokenAsync(user.Uid);

                return Ok(new { 
                    Token = token,
                    User = new {
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
    }

    public class RegisterModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class VerifyTokenModel
    {
        public string Token { get; set; } = string.Empty;
    }
}