using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using DevExpress.Xpo.Metadata;
using GestionaleRendicontazione.Domain.Entities;
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
// 4. SEED INIZIALE DEI DATI (Sfruttando il nuovo XpoContextService)
// ---------------------------------------------------------------------
// Recuperiamo il servizio scoped appena configurato per eseguire l'inizializzazione
using (var scope = app.Services.CreateScope())
{
    var xpoContext = scope.ServiceProvider.GetRequiredService<IDbContextService>();

    // Usiamo la Lambda transazionale asincrona per controllare e inserire i dati
    await xpoContext.ExecuteTransactionAsync(async uow =>
    {
        // Se non ci sono aziende, popoliamo il DB
        if (!uow.Query<Company>().Any())
        {
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
                Date = DateTime.UtcNow,
                CreateAt = DateTime.UtcNow,
                UpdateAt = DateTime.UtcNow,
                Project = project,
                Type = type,
                Status = status2,
            };
            
        }
        
        await Task.CompletedTask;
    });
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
