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

        uow.CommitChanges(); // fondamentale: senza Commit, XPO non scrive nulla su disco
    }
}

app.Run();

