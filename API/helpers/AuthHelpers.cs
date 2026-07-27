using System.Security.Claims;

namespace GestionaleRendicontazione.Api.Helpers
{
    public static class AuthHelpers
    {
        /// <summary>
        /// Restituisce il nome utente "leggibile" dal token JWT.
        /// Ritorna null se il principal è nullo o non autenticato, così il
        /// chiamante può decidere se loggare l'azione come anonima
        /// (es. "Anonymous") o saltarla del tutto.
        /// </summary>
        public static string? GetUserName(this ClaimsPrincipal? user)
        {
            if (user is null || user.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            return user.FindFirst(ClaimTypes.Name)?.Value
                ?? user.FindFirst("unique_name")?.Value
                ?? user.Identity.Name;
        }
    }
}
