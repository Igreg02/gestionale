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
using AutoMapper;
using GestionaleRendicontazione.Dataaccess.Helpers;
using GestionaleRendicontazione.Api.Helpers;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "XpoProvider=SQLite;Data Source=rendicontazione.db;";

// ---------------------------------------------------------------------
// COSTRUZIONE DATALAYER XPO (fatto qui, PRIMA di Serilog, perché il sink
// XpoSerilogSink ha bisogno di un'istanza di IDataLayer già pronta)
// ---------------------------------------------------------------------
XPDictionary xpoDictionary = new ReflectionDictionary();
xpoDictionary.GetDataStoreSchema(typeof(WorkLog).Assembly);
IDataStore xpoStore = XpoDefault.GetConnectionProvider(connectionString, AutoCreateOption.DatabaseAndSchema);
IDataLayer dataLayer = new ThreadSafeDataLayer(xpoDictionary, xpoStore);

// ---------------------------------------------------------------------
// CONFIGURAZIONE SERILOG (Console + DB)
// ---------------------------------------------------------------------
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Sink(new XpoSerilogSink(dataLayer))
    .CreateLogger();

builder.Host.UseSerilog(); // Sostituisce il logger di default con Serilog

builder.Services.AddXpoInfrastructure(dataLayer);
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

// Autenticazione JWT
var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.Configure<JwtOptions>(jwtSection);

var jwtSecretKey = jwtSection["SecretKey"] ?? string.Empty;
var jwtIssuer = jwtSection["Issuer"] ?? string.Empty;
var jwtAudience = jwtSection["Audience"] ?? string.Empty;

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
builder.Services.AddSingleton<PasswordHasher<Employee>>();
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

// Servizi applicativi
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ---------------------------------------------------------------------
// USE EXCEPTION HANDLER (Logga sia su Console che su DB XPO)
// ---------------------------------------------------------------------
app.UseExceptionHandler(handlerApp =>
{
    handlerApp.Run(async context =>
    {
        var exceptionFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        var exception = exceptionFeature?.Error;

        // 1. Log tramite ILogger (Console, via Serilog)
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        if (exception is not null)
        {
            logger.LogError(exception, "Unhandled exception during {Method} {Path}",
                context.Request.Method, context.Request.Path);

            // 2. SALVATAGGIO SU DB (XPO)
            try
            {
                // Recuperiamo l'User ID dai Claim del token JWT
                var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? context.User?.FindFirst("sub")?.Value
                 ?? "Anonymus"; // Se l'utente non è loggato (es. endpoint pubblici)

                var dataLayer = context.RequestServices.GetRequiredService<IDataLayer>();
                using (var uow = new UnitOfWork(dataLayer))
                {
                    var logDb = new LogApplicativo(uow)
                    {
                        Data = DateTime.Now,
                        Livello = "Error",
                        Messaggio = exception.Message,
                        Metodo = context.Request.Method,
                        Path = context.Request.Path,
                        StackTrace = exception.StackTrace ?? string.Empty,
                        UserId = userId
                    };
                    await uow.CommitChangesAsync();
                }
            }
            catch (Exception xpoEx)
            {
                // Se il DB è offline, questo backup di log su console ti salva la vita!
                logger.LogError(xpoEx, "Impossibile salvare il log dell'eccezione sul Database.");
            }
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

        if (app.Environment.IsDevelopment())
        {
            problem.Extensions["traceId"] = context.TraceIdentifier;
            problem.Extensions["stackTrace"] = exception?.StackTrace;
        }

        await context.Response.WriteAsJsonAsync(problem);
    });
});

app.UseAuthentication();
app.UseAuthorization();

// ---------------------------------------------------------------------
// ARRICCHIMENTO LOG: rendiamo disponibili UserId, Path e Metodo a
// qualunque log emesso durante la richiesta (letti poi da XpoSerilogSink)
// ---------------------------------------------------------------------
app.Use(async (context, next) =>
{
    var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
        ?? context.User?.FindFirst("sub")?.Value
        ?? "Anonymus";

    using (Serilog.Context.LogContext.PushProperty("UserId", userId))
    using (Serilog.Context.LogContext.PushProperty("RequestPath", context.Request.Path.ToString()))
    using (Serilog.Context.LogContext.PushProperty("RequestMethod", context.Request.Method))
    {
        await next();
    }
});

app.MapControllers();
app.UseStatusCodePages();

using (var scope = app.Services.CreateScope())
{
    var dbContextService = scope.ServiceProvider.GetRequiredService<IDbContextService>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<PasswordHasher<Employee>>();
    await DataSeeder.SeedAsync(dbContextService, passwordHasher);
}

app.Run();

// ---------------------------------------------------------------------
// EXTENSION METHODS XPO
// ---------------------------------------------------------------------
public static class XpoProgramExtensions
{
    public static IServiceCollection AddXpoInfrastructure(this IServiceCollection services, IDataLayer dataLayer)
    {
        // Registriamo l'istanza già costruita in Program.cs (serve la stessa
        // istanza usata dallo XpoSerilogSink, non una nuova per ogni richiesta)
        services.AddSingleton(dataLayer);

        services.AddScoped<IDbContextService, DbContextService>();

        return services;
    }
}