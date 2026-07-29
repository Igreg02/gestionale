using GestionaleRendicontazione.Api.Helpers;
using GestionaleRendicontazione.Api.Helpers.ProblemDetails;
using GestionaleRendicontazione.Api.Services;
using GestionaleRendicontazione.Api.Services.Cors;
using GestionaleRendicontazione.Api.Services.Jwt;
using GestionaleRendicontazione.Api.Services.Swagger;
using GestionaleRendicontazione.Api.Services.Xpo;
using GestionaleRendicontazione.Dataaccess.Datacontext;
using GestionaleRendicontazione.Domain.Entities;
using GestionaleRendicontazione.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
Serilog.Debugging.SelfLog.Enable(msg => Console.Error.WriteLine(msg));

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "XpoProvider=SQLite;Data Source=rendicontazione.db;";

var dataLayer = XpoBootstrap.BuildDataLayer(connectionString);

var httpContextAccessor = new HttpContextAccessor();
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.With(new RequestContextEnricher(httpContextAccessor))
    .WriteTo.Console()
    .WriteTo.Sink(new XpoSerilogSink(dataLayer))
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddHttpContextAccessor();
builder.Services.AddXpoInfrastructure(dataLayer);
builder.Services.AddBlazorClientCors(builder.Configuration);
builder.Services.AddApiDocumentation();
builder.Services.AddJwtAuthenticationWithBlacklist(builder.Configuration);
builder.Services.AddApplicationServices();

var app = builder.Build();

app.UseCors(BlazorClientCorsExtensions.PolicyName);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseGlobalProblemDetails();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseStatusCodePages();

using (var scope = app.Services.CreateScope())
{
    var dbContextService = scope.ServiceProvider.GetRequiredService<IDbContextService>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<PasswordHasher<Employee>>();
    await DataSeeder.SeedAsync(dbContextService, passwordHasher, CancellationToken.None);
}

app.Run();
