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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.IdentityModel.Tokens;
using AutoMapper;
using GestionaleRendicontazione.Dataaccess.Helpers;
using GestionaleRendicontazione.Api.Helpers;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
Serilog.Debugging.SelfLog.Enable(msg => Console.Error.WriteLine(msg));

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "XpoProvider=SQLite;Data Source=rendicontazione.db;";

// ---------------------------------------------------------------------
// COSTRUZIONE DATALAYER XPO (fatto qui, PRIMA di Serilog, perché il sink
// XpoSerilogSink ha bisogno di un'istanza di IDataLayer già pronta)
// ---------------------------------------------------------------------
EnableSqliteWalMode(connectionString);
XPDictionary xpoDictionary = new ReflectionDictionary();
xpoDictionary.GetDataStoreSchema(typeof(WorkLog).Assembly);
IDataStore xpoStore = XpoDefault.GetConnectionProvider(connectionString, AutoCreateOption.DatabaseAndSchema);
IDataLayer dataLayer = new ThreadSafeDataLayer(xpoDictionary, xpoStore);

var httpContextAccessor = new HttpContextAccessor();
builder.Services.AddSingleton<IHttpContextAccessor>(httpContextAccessor);

// ---------------------------------------------------------------------
// CONFIGURAZIONE SERILOG (Console + DB)
// ---------------------------------------------------------------------
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.With(new RequestContextEnricher(httpContextAccessor))
    .WriteTo.Console()
    .WriteTo.Sink(new XpoSerilogSink(dataLayer))
    .CreateLogger();

builder.Host.UseSerilog(); // Sostituisce il logger di default con Serilog

builder.Services.AddXpoInfrastructure(dataLayer);

// ---------------------------------------------------------------------
// CORS — necessario a partire dalla Fase F1 del frontend Blazor WebAssembly,
// che gira su un'origine diversa (es. https://localhost:7210) da quella
// dell'API. Le origini consentite sono in appsettings.json ("Cors:AllowedOrigins"),
// così da poter differenziare sviluppo/produzione senza toccare il codice.
// ---------------------------------------------------------------------
const string BlazorClientCorsPolicy = "BlazorClient";
var corsAllowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(BlazorClientCorsPolicy, policy =>
    {
        policy.WithOrigins(corsAllowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

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

// CORS deve essere il primo middleware della pipeline: deve intercettare
// la richiesta (ed eseguire eventuale preflight OPTIONS) prima di qualunque
// altro middleware che possa generare direttamente la risposta — Swagger
// compreso — altrimenti l'header Access-Control-Allow-Origin non viene mai
// aggiunto e il browser blocca la risposta lato client.
app.UseCors(BlazorClientCorsPolicy);

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

        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
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
        services.AddSingleton(dataLayer);
        services.AddScoped<IDbContextService, DbContextService>();
        return services;
    }
}

// ---------------------------------------------------------------------
// SQLITE WAL MODE
// ---------------------------------------------------------------------
// SQLite di default usa un rollback journal, che durante una scrittura blocca
// tutte le letture concorrenti sullo stesso file. Il WAL mode permette letture
// concorrenti mentre è in corso una scrittura, riducendo il rischio di
// "database is locked" quando più richieste arrivano insieme.
partial class Program
{
    static void EnableSqliteWalMode(string xpoConnectionString)
    {
        var sqliteConnectionString = xpoConnectionString.Replace("XpoProvider=SQLite;", string.Empty);
        if (!sqliteConnectionString.Contains("Data Source", StringComparison.OrdinalIgnoreCase))
        {
            return; // Non è SQLite (es. altro provider XPO): niente da fare
        }

        using var connection = new SqliteConnection(sqliteConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA journal_mode=WAL;";
        command.ExecuteNonQuery();
    }
}