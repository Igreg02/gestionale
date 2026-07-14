using System.Text;
using Microsoft.OpenApi;
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
using AutoMapper;
using GestionaleRendicontazione.Dataaccess.Helpers;

var builder = WebApplication.CreateBuilder(args);

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "XpoProvider=SQLite;Data Source=rendicontazione.db;";

builder.Services.AddXpoInfrastructure(connectionString);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.OpenApiSecurityScheme
    {
        Description = "Inserisci unicamente il token JWT (il prefisso 'Bearer ' verrà aggiunto in automatico).",
        Name = "Authorization",
        In = Microsoft.OpenApi.ParameterLocation.Header,
        Type = Microsoft.OpenApi.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    options.AddSecurityRequirement(document => new Microsoft.OpenApi.OpenApiSecurityRequirement
    {
        [new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });
});
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
            NameClaimType = System.Security.Claims.ClaimTypes.NameIdentifier
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var blacklist = context.HttpContext.RequestServices.GetRequiredService<ITokenBlacklistService>();
                var jti = context.Principal?.FindFirst("jti")?.Value;
                if (jti != null && await blacklist.IsBlacklistedAsync(jti))
                {
                    context.Fail("Token revocato.");
                }
            }
        };
    });

builder.Services.AddAuthorization();

// PasswordHasher di Microsoft.Extensions.Identity: usato da AuthService per
// hashare e verificare la password degli Employee.
builder.Services.AddSingleton<PasswordHasher<Employee>>();

// Registrazione AutoMapper
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

// Servizi applicativi.
builder.Services.AddScoped<ITokenBlacklistService, TokenBlacklistService>();
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
// app.UseHttpsRedirection(); // Disabilitato in Development per permettere HTTP

// FIX #4: UseExceptionHandler reale.
// Cattura qualsiasi eccezione non gestita nei controller e produce una risposta
// ProblemDetails coerente. Mappa le InvalidOperationException (solitamente lanciate
// dai service per "FK mancanti", "vincolo dipendenze", "username duplicato", …)
// a 409 Conflict, lasciando il resto a 500. Viene loggato tutto.
app.UseExceptionHandler(builder =>
{
    builder.Run(async context =>
    {
        var exceptionFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        var exception = exceptionFeature?.Error;
        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger("GlobalExceptionHandler");

        if (exception is not null)
        {
            logger.LogError(exception, "Unhandled exception during {Method} {Path}",
                context.Request.Method, context.Request.Path);
        }

        var (status, title) = exception switch
        {
            InvalidOperationException => (StatusCodes.Status409Conflict, "Operazione non valida"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Non autorizzato"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Richiesta non valida"),
            _ => (StatusCodes.Status500InternalServerError, "Errore interno del server")
        };

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";

        var problem = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Status = status,
            Title = title,
            Type = $"https://httpstatuses.io/{status}",
            Detail = exception?.Message,
            Instance = context.Request.Path
        };

        if (app.Services
            .GetRequiredService<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>()
            .IsDevelopment())
        {
            problem.Extensions["traceId"] = context.TraceIdentifier;
            problem.Extensions["stackTrace"] = exception?.StackTrace;
        }

        await context.Response.WriteAsJsonAsync(problem);
    });
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseStatusCodePages();


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
