using System.Security.Claims;

namespace GestionaleRendicontazione.Api.Helpers
{
    public static class ClaimsPrincipalExtensions
    {
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

        public static Guid? GetEmployeeId(this ClaimsPrincipal user)
        {
            if (user?.Identity?.IsAuthenticated != true) return null;
            var raw = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(raw, out var parsed) ? parsed : (Guid?)null;
        }
    }
}
