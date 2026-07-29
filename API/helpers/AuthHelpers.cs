using System.Security.Claims;

namespace GestionaleRendicontazione.Api.Helpers
{
    public static class AuthHelpers
    {
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
