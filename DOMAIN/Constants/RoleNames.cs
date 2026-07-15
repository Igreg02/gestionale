namespace GestionaleRendicontazione.Domain.Constants
{
    /// <summary>
    /// Nomi dei ruoli applicativi (corrispondono ai record <c>PermissionPolicyRole.Name</c> su database).
    /// Centralizzati qui per evitare stringhe magiche duplicate tra AuthService, DataSeeder
    /// e gli attributi [Authorize(Roles = "...")] dei controller.
    /// </summary>
    public static class RoleNames
    {
        public const string Admin = "Admin";
        public const string User = "User";
    }
}
