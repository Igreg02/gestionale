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
        private readonly int _expiryMinutes; 

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
            
            // Salva solo i minuti nel costruttore
            _expiryMinutes = expiryMinutes <= 0 ? 60 : expiryMinutes;
        }

        public string CreateToken(IEnumerable<Claim> claims)
        {
            var now = DateTime.UtcNow;
            
            // Calcolo dinamico ad ogni login
            var expiresAt = now.AddMinutes(_expiryMinutes); 

            var jwt = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                notBefore: now,
                expires: expiresAt,
                signingCredentials: _signingCredentials);

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        // Calcola la scadenza relativa al momento esatto della richiesta
        public DateTime GetExpiry() => DateTime.UtcNow.AddMinutes(_expiryMinutes);
    }
}
