using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using DevExpress.Xpo.Metadata;
using GestionaleRendicontazione.Domain.Entities;
using GestionaleRendicontazione.Dataaccess.Datacontext;
using GestionaleRendicontazione.Dataaccess.Datacontext.DbContextService;

var builder = WebApplication.CreateBuilder(args);

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "XpoProvider=SQLite;Data Source=rendicontazione.db;";

builder.Services.AddXpoInfrastructure(connectionString);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        // parametri di convalida JWT
    });

var app = builder.Build();


if (app.Environment.IsDevelopment())
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
    await DataSeeder.SeedAsync(dbContextService);
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
