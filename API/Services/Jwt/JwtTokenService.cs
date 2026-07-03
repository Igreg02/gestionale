using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GestionaleRendicontazione.Domain.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace GestionaleRendicontazione.Api.Services.Jwt
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly SigningCredentials _signingCredentials;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly DateTime _expiry;

        public JwtTokenService(IConfiguration configuration)
        {
            var issuer = configuration["Jwt:Issuer"];
            var audience = configuration["Jwt:Audience"];
            var secretKey = configuration["Jwt:SecretKey"];
            var expiryMinutes = int.TryParse(configuration["Jwt:ExpiryMinutes"], out var m) ? m : 60;

            if (string.IsNullOrWhiteSpace(secretKey) || secretKey.Length < 32)
            {
                throw new InvalidOperationException(
                    "Jwt:SecretKey non configurata o troppo corta (richiesti almeno 32 caratteri). " +
                    "Impostarla in appsettings.json o tramite variabile d'ambiente Jwt__SecretKey.");
            }

            _issuer = issuer ?? string.Empty;
            _audience = audience ?? string.Empty;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            _signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            _expiry = DateTime.UtcNow.AddMinutes(expiryMinutes <= 0 ? 60 : expiryMinutes);
        }

        public string CreateToken(IEnumerable<Claim> claims)
        {
            var jwt = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: _expiry,
                signingCredentials: _signingCredentials);

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        public DateTime GetExpiry() => _expiry;
    }
}
