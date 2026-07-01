using DevExpress.Xpo;
using DevExpress.Xpo.DB;

var builder = WebApplication.CreateBuilder(args);

// 1. Configura la stringa di connessione (es. SQLite)
string connectionString = "XpoProvider=Sqlite;Data Source=rendicontazione.db";

// 2. Inizializza il Data Layer di XPO (approccio ThreadSafe per ambienti Web)
XpoDefault.DataLayer = XpoDefault.GetDataLayer(
    connectionString, 
    AutoCreateOption.DatabaseAndSchema
);

// 3. Registra l'UnitOfWork come Scoped (uno per ogni richiesta HTTP)
builder.Services.AddScoped<UnitOfWork>(sp => new UnitOfWork(XpoDefault.DataLayer));

builder.Services.AddControllers();