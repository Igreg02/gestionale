using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GestionaleRendicontazione.Domain.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GestionaleRendicontazione.Api.Services.Jwt
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly SigningCredentials _signingCredentials;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expiryMinutes;

        public JwtTokenService(IOptions<JwtOptions> options)
        {
            var jwt = options.Value;

            if (string.IsNullOrWhiteSpace(jwt.SecretKey) || jwt.SecretKey.Length < 32)
            {
                throw new InvalidOperationException(
                    "Jwt:SecretKey non configurata o troppo corta (richiesti almeno 32 caratteri). " +
                    "Impostarla in appsettings.json o tramite variabile d'ambiente Jwt__SecretKey.");
            }

            _issuer = jwt.Issuer ?? string.Empty;
            _audience = jwt.Audience ?? string.Empty;
            _expiryMinutes = jwt.ExpiryMinutes <= 0 ? 60 : jwt.ExpiryMinutes;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey));
            _signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        }

        public string CreateToken(IEnumerable<Claim> claims)
        {
            var now = DateTime.UtcNow;
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

        public DateTime GetExpiry() => DateTime.UtcNow.AddMinutes(_expiryMinutes);
    }
}
