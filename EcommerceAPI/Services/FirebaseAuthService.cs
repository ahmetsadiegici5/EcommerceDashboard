using FirebaseAdmin.Auth;

namespace EcommerceAPI.Services
{
    public class FirebaseAuthService
    {
        private readonly FirebaseAuth _firebaseAuth;
        private readonly IConfiguration _configuration;

        public FirebaseAuthService(IConfiguration configuration)
        {
            _firebaseAuth = FirebaseAuth.DefaultInstance;
            _configuration = configuration;
        }

        public async Task<UserRecord> CreateUserAsync(string email, string password)
        {
            var args = new UserRecordArgs
            {
                Email = email,
                Password = password,
                EmailVerified = false,
                Disabled = false
            };

            return await _firebaseAuth.CreateUserAsync(args);
        }

        public async Task<string> CreateTokenAsync(string uid)
        {
            var customClaims = new Dictionary<string, object>
            {
                { "role", "seller" }
            };

            await _firebaseAuth.SetCustomUserClaimsAsync(uid, customClaims);
            return await _firebaseAuth.CreateCustomTokenAsync(uid);
        }

        public async Task<UserRecord> GetUserAsync(string uid)
        {
            return await _firebaseAuth.GetUserAsync(uid);
        }

        public async Task<FirebaseToken> VerifyTokenAsync(string token)
        {
            return await _firebaseAuth.VerifyIdTokenAsync(token);
        }
    }
}