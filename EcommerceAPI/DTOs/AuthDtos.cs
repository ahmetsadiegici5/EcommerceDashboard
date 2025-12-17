namespace EcommerceAPI.DTOs
{
    public class RegisterModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class VerifyTokenModel
    {
        public string Token { get; set; } = string.Empty;
    }

    public class SessionRequest
    {
        public string IdToken { get; set; } = string.Empty;
    }
}
