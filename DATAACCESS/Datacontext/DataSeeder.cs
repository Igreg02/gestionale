using DevExpress.Xpo;
using GestionaleRendicontazione.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using GestionaleRendicontazione.Domain.Interfaces;

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
                    var adminRole = uow.Query<PermissionPolicyRole>().FirstOrDefault(r => r.Name == "Admin");
                    if (adminRole == null)
                    {
                        adminRole = new PermissionPolicyRole(uow)
                        {
                            Name = "Admin",
                        };
                    }

                    var userRole = uow.Query<PermissionPolicyRole>().FirstOrDefault(r => r.Name == "User");
                    if (userRole == null)
                    {
                        userRole = new PermissionPolicyRole(uow)
                        {
                            Name = "User",
                        };
                    }

                    //if (uow.Query<Employee>().Any(e => e.UserName == "admin") == false)
                    //{
                    var admin = new Employee(uow)
                    {
                        UserName = "admin",
                        FirstName = "Admin",
                        LastName = "Default",
                        IsActive = true,
                        PasswordHash = passwordHasher.HashPassword(null!, "Admin123!")
                    };
                    admin.Roles.Add(adminRole);
                    await uow.CommitChangesAsync();
                    //}

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
                        Date = DateOnly.FromDateTime(DateTime.UtcNow),
                        CreateAt = DateTime.UtcNow,
                        UpdateAt = DateTime.UtcNow,
                        Project = project,
                        Type = type,
                        Status = status2,
                        Employee = admin,
                        IsWorkLogDeleted = false
                    };
                }
                await Task.CompletedTask;
            });
        }
    }
}

