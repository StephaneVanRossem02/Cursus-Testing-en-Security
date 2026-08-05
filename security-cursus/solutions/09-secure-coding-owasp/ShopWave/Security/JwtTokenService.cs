using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace ShopWave.Security
{
    public class JwtTokenService
    {
        private readonly string secretKey;
        private readonly string issuer;
        private readonly string audience;
        private readonly int    expiresMinutes;

        public JwtTokenService(string secretKey, string issuer, string audience, int expiresMinutes = 30)
        {
            this.secretKey      = secretKey;
            this.issuer         = issuer;
            this.audience       = audience;
            this.expiresMinutes = expiresMinutes;
        }

        public string GenerateToken(string email, string role)
        {
            byte[]               keyBytes    = Encoding.UTF8.GetBytes(secretKey);
            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(keyBytes);
            SigningCredentials   credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            List<Claim> claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Iat,
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
            };

            JwtSecurityToken token = new JwtSecurityToken(
                issuer:             issuer,
                audience:           audience,
                claims:             claims,
                expires:            DateTime.UtcNow.AddMinutes(expiresMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
