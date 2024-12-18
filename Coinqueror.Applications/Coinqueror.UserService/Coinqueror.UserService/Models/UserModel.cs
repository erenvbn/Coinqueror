namespace Coinqueror.UserService.Models
{
    public class UserModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public string? Token { get; set; }
        public string BinanceApiKeyHash { get; set; }
        public string BinanceApiKeySalt { get; set; }
        public string BinanceApiSecretHash { get; set; }
        public string BinanceApiSecretSalt { get; set; }
        public DateTime? LastLoginDate { get; set; } // Nullable to indicate if the user has never logged in
        public DateTime? LastLoginExpiryDate { get; set; } // Nullable to indicate if the user has never logged in
        public DateTime? LastLogoutDate { get; set; } // Nullable to indicate if the user has never logged in
    }
}
