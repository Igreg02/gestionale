using System.Security.Claims;
using System.Text.Json;

namespace GestionaleRendicontazione.Client.Services
{
    /// <summary>
    /// Decodifica il payload di un JWT per ricavarne i claim (ruoli, nome utente, scadenza) e
    /// popolare il <see cref="ClaimsPrincipal"/> lato client. NON valida la firma: la validazione
    /// del token resta esclusivamente responsabilità del backend (vedi TDD §2.3, JwtBearerEvents);
    /// qui serve solo a leggere informazioni già presenti in un token di cui ci si è appena fidati.
    /// </summary>
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
                    // Con più ruoli, JwtSecurityToken serializza il claim "role" come array JSON.
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

        /// <summary>
        /// Il backend costruisce i claim con i tipi standard di ClaimTypes (Name, NameIdentifier, Role):
        /// a seconda di come JwtPayload li serializza possono comparire nel token sia in forma "corta"
        /// (unique_name, nameid, role) sia con l'URI lungo di ClaimTypes.*. Normalizziamo entrambe le
        /// forme verso ClaimTypes.*, così AuthorizeView/AuthorizeRouteView funzionano senza sorprese.
        /// </summary>
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
