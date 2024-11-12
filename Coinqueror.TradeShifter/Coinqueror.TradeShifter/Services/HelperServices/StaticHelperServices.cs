using Coinqueror.TradeShifter.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Coinqueror.TradeShifter.Services.HelperServices
{
    public static class StaticHelperServices
    {
        public static string GenerateJwtToken(UserModel user, int expiryDaysToAdd, string superSecretKey)
        {
            string jwtKeyString = superSecretKey;
            var key = Encoding.ASCII.GetBytes(jwtKeyString);
            // Define your token handler and secret key
            var tokenHandler = new JwtSecurityTokenHandler();
            //var key = Encoding.ASCII.GetBytes("YourVerySuperSecretKeyThatIsLongEnough"); // Use a secure key

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email)
                }),
                Expires = DateTime.UtcNow.AddDays(expiryDaysToAdd), // Set token expiration
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
