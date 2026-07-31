using DevExpress.Xpo;
using GestionaleRendicontazione.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using GestionaleRendicontazione.Domain.Interfaces;
using GestionaleRendicontazione.Domain.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace GestionaleRendicontazione.Dataaccess.Datacontext
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(
            IDbContextService DbContextService,
            PasswordHasher<Employee> passwordHasher,
            IConfiguration configuration,
            ILogger logger,
            CancellationToken ct = default)
        {
            var adminPassword = configuration["Seed:AdminPassword"];
            var passwordWasGenerated = string.IsNullOrWhiteSpace(adminPassword);
            if (passwordWasGenerated)
            {
                adminPassword = GenerateRandomPassword();
            }

            var seeded = false;

            await DbContextService.ReadWriteAsync(async uow =>
            {
                if (uow.Query<Company>().Any())
                {
                    return;
                }

                seeded = true;

                var adminRole = uow.Query<PermissionPolicyRole>().FirstOrDefault(r => r.Name == RoleNames.Admin);
                if (adminRole == null)
                {
                    adminRole = new PermissionPolicyRole(uow)
                    {
                        Name = RoleNames.Admin,
                    };
                }

                var userRole = uow.Query<PermissionPolicyRole>().FirstOrDefault(r => r.Name == RoleNames.User);
                if (userRole == null)
                {
                    userRole = new PermissionPolicyRole(uow)
                    {
                        Name = RoleNames.User,
                    };
                }

                var admin = new Employee(uow)
                {
                    UserName = "admin",
                    FirstName = "Admin",
                    LastName = "Default",
                    IsActive = true,
                    PasswordHash = passwordHasher.HashPassword(null!, adminPassword!)
                };
                admin.Roles.Add(adminRole);

                var company = new Company(uow)
                {
                    Name = "Azienda Demo",
                };

                var project = new Project(uow)
                {
                    Name = "Progetto Demo",
                    Company = company
                };

                var type = new GestionaleRendicontazione.Domain.Entities.Type(uow)
                {
                    Name = "FIX"
                };

                var status = new Status(uow)
                {
                    Name = "WORKING_PROGRESS"
                };

                var status2 = new Status(uow)
                {
                    Name = "REJECTED"
                };

                // 3) Worklog dimostrativo: Employee = admin, non più orfano.
                //    La FK verrà materializzata al commit finale centralizzato.
                var worklog = new WorkLog(uow)
                {
                    Description = "Descrizione",
                    HoursCounter = 2,
                    Date = DateOnly.FromDateTime(DateTime.UtcNow),
                    CreateAt = DateTime.UtcNow,
                    UpdateAt = DateTime.UtcNow,
                    Project = project,
                    Type = type,
                    Status = status2,
                    Employee = admin,
                    IsWorkLogDeleted = false
                };
            });

            if (seeded && passwordWasGenerated)
            {
                logger.LogWarning(
                    "Utente admin creato con password generata automaticamente: {AdminPassword}. " +
                    "Cambiarla al primo accesso. Per fissarla esplicitamente, impostare Seed:AdminPassword (config o env var Seed__AdminPassword).",
                    adminPassword);

                // Il log console/DB si può perdere (scroll, log ruotati). La password non è più
                // recuperabile una volta salvato solo l'hash, quindi la scriviamo anche qui:
                // unico posto pensato per essere letto UNA volta e cancellato subito dopo.
                try
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "ADMIN_PASSWORD_FIRST_RUN.txt");
                    File.WriteAllText(filePath,
                        $"Utente: admin{Environment.NewLine}" +
                        $"Password generata automaticamente: {adminPassword}{Environment.NewLine}" +
                        $"Cambiarla al primo accesso, poi cancellare questo file.{Environment.NewLine}");
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Impossibile scrivere ADMIN_PASSWORD_FIRST_RUN.txt (password comunque disponibile nel log qui sopra)");
                }
            }
        }

        private static string GenerateRandomPassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@#$%";
            var bytes = RandomNumberGenerator.GetBytes(24);
            var result = new char[24];
            for (int i = 0; i < bytes.Length; i++)
            {
                result[i] = chars[bytes[i] % chars.Length];
            }
            return new string(result);
        }
    }
}