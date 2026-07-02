using DevExpress.Xpo;
using GestionaleRendicontazione.Domain.Entities;
using GestionaleRendicontazione.Dataaccess.Datacontext.DbContextService;
using Microsoft.AspNetCore.Identity;

namespace GestionaleRendicontazione.Dataaccess.Datacontext
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(IDbContextService DbContextService, PasswordHasher<Employee> passwordHasher)
        {
            await DbContextService.ReadWrite(async uow =>
            {
                if (!uow.Query<Company>().Any() && true) /* true -> scrive in assenza di dati | false -> scrive sempre*/
                {
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

                    var worklog = new WorkLog(uow)
                    {
                        Description = "Descrizione",
                        HoursCounter = 2,
                        Date = DateTime.UtcNow,
                        CreateAt = DateTime.UtcNow,
                        UpdateAt = DateTime.UtcNow,
                        Project = project,
                        Type = type,
                        Status = status2,
                    };
                }

                // Seed utente admin di default (idempotente: salta se esiste già un Employee con questo UserName).
                if (uow.Query<Employee>().Any(e => e.UserName == "admin") == false)
                {
                    var admin = new Employee(uow)
                    {
                        UserName = "admin",
                        FirstName = "Admin",
                        LastName = "Default",
                        HireDate = DateTime.UtcNow,
                        IsActive = true,
                        PasswordHash = passwordHasher.HashPassword(null!, "Admin123!")
                    };
                }

                await Task.CompletedTask;
            });
        }
    }
}

