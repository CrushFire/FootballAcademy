using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Utils
{
    public class JwtTokenGen
    {
        private readonly string _audience;
        private readonly string _issuer;
        private readonly string _key;
        public JwtTokenGen(IConfiguration configuration)
        {
            _key = configuration["Jwt:Key"];
            _issuer = configuration["Jwt:Issuer"];
            _audience = configuration["Jwt:Audience"];
        }

        public string TokenGen(string nameIdentifier, string userName, string userEmail, string userRole, string? personalType = null, string? personalId = null)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_key);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, nameIdentifier ?? string.Empty),
                new Claim(ClaimTypes.Name, userName ?? string.Empty),
                new Claim(ClaimTypes.Email, userEmail ?? string.Empty),
                new Claim(ClaimTypes.Role, userRole ?? string.Empty)
            };
            if (personalType != null)
                claims.Add(new Claim("personalType", personalType));
            if (personalId != null)
                claims.Add(new Claim("personalId", personalId));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(120),
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
