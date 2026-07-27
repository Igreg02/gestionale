using System.Security.Claims;

namespace GestionaleRendicontazione.Api.Helpers
{
    public static class ClaimsPrincipalExtensions
    {
        // Recupera l'identificativo dell'utente autenticato dai claim del token JWT.
        // Se la richiesta non è autenticata (es. endpoint pubblici) restituisce
        // "Anonymous", così possiamo distinguere le azioni di un visitatore non
        // loggato da quelle interne al sistema (etichettate invece "system"
        // dall'enricher, che lavora fuori da un HttpContext).
        public static string GetUserId(this ClaimsPrincipal? user)
        {
            if (user?.Identity?.IsAuthenticated == true)
            {
                return user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? user.FindFirst("sub")?.Value
                    ?? "Anonymous";
            }

            return "Anonymous";
        }
    }
}
