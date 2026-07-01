using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using DevExpress.Xpo.Metadata;
using GestionaleRendicontazione.Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------
// 1. CONFIGURAZIONE DI DEVEXPRESS XPO (DATA LAYER)
// ---------------------------------------------------------------------
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "XpoProvider=SQLite;Data Source=rendicontazione.db;"; // ✅ formato corretto per XPO

var entityTypes = new[] {
    typeof(Company),
    typeof(Project)
};

var dictionary = new ReflectionDictionary();
foreach (var t in entityTypes)
    dictionary.GetClassInfo(t);

XpoDefault.DataLayer = XpoDefault.GetDataLayer(
    connectionString,
    dictionary,
    AutoCreateOption.DatabaseAndSchema
);

builder.Services.AddScoped<UnitOfWork>(sp =>
{
    return new UnitOfWork(XpoDefault.DataLayer);
});

// ---------------------------------------------------------------------
// 2. REGISTRAZIONE DEI SERVIZI APPLICATIVI (DI)
// ---------------------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        // parametri di convalida JWT
    });

var app = builder.Build();

// ---------------------------------------------------------------------
// 3. CONFIGURAZIONE DELLA PIPELINE MIDDLEWARE (HTTP)
// ---------------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseExceptionHandler(opt => { });

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


// ---------------------------------------------------------------------
// SEED INIZIALE DEI DATI (solo se il DB è vuoto)
// ---------------------------------------------------------------------
using (var uow = new UnitOfWork(XpoDefault.DataLayer))
{
    if (!uow.Query<Company>().Any())
    {
        var company = new Company(uow)
        {
            Name = "Azienda Demo",
            // altri campi obbligatori...
        };

        var project = new Project(uow)
        {
            Name = "Progetto Demo",
            Company = company
            // altri campi...
        };

        var type = new GestionaleRendicontazione.Domain.Entities.Type(uow) // Più verboso perchè esiste System.Type
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
            updateAt = DateTime.UtcNow,
            Project = project,
            Type = type,
            Status = status2,
        };

        uow.CommitChanges(); // fondamentale: senza Commit, XPO non scrive nulla su disco
    }
}

app.Run();

