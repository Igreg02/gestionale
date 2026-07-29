using System.Security.Claims;
using System.Text.Json;

namespace GestionaleRendicontazione.Client.Services
{
    internal static class JwtParser
    {
        public static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            var parts = jwt.Split('.');
            if (parts.Length < 2)
            {
                return claims;
            }

            var jsonBytes = Base64UrlDecode(parts[1]);
            using var doc = JsonDocument.Parse(jsonBytes);

            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                var claimType = MapClaimType(prop.Name);

                if (prop.Value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in prop.Value.EnumerateArray())
                    {
                        claims.Add(new Claim(claimType, item.ToString()));
                    }
                }
                else
                {
                    claims.Add(new Claim(claimType, prop.Value.ToString()));
                }
            }

            return claims;
        }

        private static string MapClaimType(string jwtClaimName) => jwtClaimName switch
        {
            "role" => ClaimTypes.Role,
            "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" => ClaimTypes.Role,
            "unique_name" => ClaimTypes.Name,
            "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name" => ClaimTypes.Name,
            "nameid" => ClaimTypes.NameIdentifier,
            "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier" => ClaimTypes.NameIdentifier,
            _ => jwtClaimName
        };

        private static byte[] Base64UrlDecode(string input)
        {
            var padded = input.Replace('-', '+').Replace('_', '/');
            switch (padded.Length % 4)
            {
                case 2: padded += "=="; break;
                case 3: padded += "="; break;
            }
            return Convert.FromBase64String(padded);
        }
    }
}
