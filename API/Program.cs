using System.Text;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using DevExpress.Xpo.Metadata;
using GestionaleRendicontazione.Api.Services.Jwt;
using GestionaleRendicontazione.Domain.Entities;
using GestionaleRendicontazione.Domain.Interfaces;
using GestionaleRendicontazione.Dataaccess.Datacontext;
using GestionaleRendicontazione.Dataaccess.Datacontext.DbContextService;
using GestionaleRendicontazione.Dataaccess.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "XpoProvider=SQLite;Data Source=rendicontazione.db;";

builder.Services.AddXpoInfrastructure(connectionString);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ---------------------------------------------------------------------
// AUTENTICAZIONE JWT self-issued (TDD §1: "JWT Bearer Token (ASP.NET Core Identity / OAuth2)")
// ---------------------------------------------------------------------
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtSecretKey = jwtSection["SecretKey"] ?? string.Empty;
var jwtIssuer = jwtSection["Issuer"] ?? string.Empty;
var jwtAudience = jwtSection["Audience"] ?? string.Empty;

// Validazione del token in ingresso. La chiave è la stessa usata in JwtTokenService
// per la firma (HMAC-SHA256). ClockSkew = 0 per non allungare artificialmente la vita del token.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
            ClockSkew = TimeSpan.Zero,
            // Mappiamo il claim "sub" sul NameIdentifier di ClaimsPrincipal, comodo per HttpContext.User.
            NameClaimType = System.Security.Claims.ClaimTypes.NameIdentifier
        };
    });

builder.Services.AddAuthorization();

// PasswordHasher di Microsoft.Extensions.Identity: usato da AuthService per
// hashare e verificare la password degli Employee.
builder.Services.AddSingleton<PasswordHasher<Employee>>();

// Servizi applicativi.
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IWorkLogAdminService, WorkLogAdminService>();
builder.Services.AddScoped<IWorkLogUserService, WorkLogUserService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IStatusService, StatusService>();
builder.Services.AddScoped<ITypeService, TypeService>();
builder.Services.AddScoped<IReportService, ReportService>();

var app = builder.Build();


if (app.Environment.IsDevelopment()) // TODO: RIMUOVERE SWAGGHER
{
    app.UseSwagger(); 
    app.UseSwaggerUI(); 
}
app.UseHttpsRedirection();

// Prima di auth per catturare eventuali errori globali di sicurezza
app.UseExceptionHandler(opt => { });

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();


// ---------------------------------------------------------------------
// 4. SEED INIZIALE DEI DATI — delegato al DataSeeder dedicato
// ---------------------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var dbContextService = scope.ServiceProvider.GetRequiredService<IDbContextService>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<PasswordHasher<Employee>>();
    await DataSeeder.SeedAsync(dbContextService, passwordHasher);
}

app.Run();




    public static class XpoProgramExtensions
    {
        public static IServiceCollection AddXpoInfrastructure(this IServiceCollection services, string connectionString)
        {
            XPDictionary dictionary = new ReflectionDictionary();

            // mappaggio xpo automatico delle Entitys
            dictionary.GetDataStoreSchema(typeof(WorkLog).Assembly);

            services.AddSingleton<IDataLayer>(sp =>
            {
                IDataStore store = XpoDefault.GetConnectionProvider(connectionString, AutoCreateOption.DatabaseAndSchema);
                return new ThreadSafeDataLayer(dictionary, store);
            });

            // Registriamo il servizio Scoped per la lettura / scrittura tramite lambda
            services.AddScoped<IDbContextService, DbContextService>();

            return services;
        }
    }
