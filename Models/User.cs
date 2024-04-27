using Microsoft.IdentityModel.Tokens;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ECommerce.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [StringLength(maximumLength:100,MinimumLength =2)]
        public required string Username { get; set; }
        [StringLength(maximumLength: 100, MinimumLength = 8)]

        public required string Password { get; set; }
        [StringLength(maximumLength: 100, MinimumLength = 3),EmailAddress]
        public required string Email { get; set; }
        public string token { get; set; } = "";

        public ICollection<Product> Products => new List<Product>();
        public static string GenerateJWTToken(string issuer, string audience,string jwtsecret,int id)
        {
            var claims = new List<Claim>{
                                new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                                new Claim(ClaimTypes.Role, "User")
                            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = issuer,
                Audience = audience,
                Expires = DateTime.UtcNow.AddDays(30), // Set token expiration
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtsecret)),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(securityToken);
            return tokenString;
        }
    }
}
