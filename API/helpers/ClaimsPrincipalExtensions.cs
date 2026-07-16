using System.Security.Claims;

namespace GestionaleRendicontazione.Api.Helpers
{
    public static class ClaimsPrincipalExtensions
    {
        // Recupera l'identificativo dell'utente autenticato dai claim del token JWT,
        // oppure "Anonymous" se la richiesta non è autenticata (es. endpoint pubblici).
        public static string GetUserId(this ClaimsPrincipal? user)
        {
            return user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? user?.FindFirst("sub")?.Value
                ?? "Anonymous";
        }
    }
}
