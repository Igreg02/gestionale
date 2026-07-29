using System;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using DevExpress.Xpo.Metadata;
using GestionaleRendicontazione.Dataaccess.Datacontext.DbContextService;
using GestionaleRendicontazione.Domain.Entities;
using GestionaleRendicontazione.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace GestionaleRendicontazione.Api.Services.Xpo
{
    /// <summary>
    /// Costruisce l'infrastruttura XPO (dataLayer + modalità WAL su SQLite)
    /// e registra i servizi che ne dipendono (IDbContextService, dataLayer singleton).
    /// Va chiamato PRIMA di configurare i sink Serilog che si appoggiano al DB,
    /// perché la costruzione del dataLayer deve avvenire in questa fase.
    /// </summary>
    public static class XpoBootstrap
    {
        public static IDataLayer BuildDataLayer(string connectionString)
        {
            EnableSqliteWalMode(connectionString);

            var dictionary = new ReflectionDictionary();
            dictionary.GetDataStoreSchema(typeof(WorkLog).Assembly);
            var dataStore = XpoDefault.GetConnectionProvider(connectionString, AutoCreateOption.DatabaseAndSchema);
            return new ThreadSafeDataLayer(dictionary, dataStore);
        }

        public static IServiceCollection AddXpoInfrastructure(
            this IServiceCollection services,
            IDataLayer dataLayer)
        {
            services.AddSingleton(dataLayer);
            services.AddScoped<IDbContextService, DbContextService>();
            return services;
        }

        // SQLite di default usa un rollback journal, che durante una scrittura blocca
        // tutte le letture concorrenti sullo stesso file. Il WAL mode permette letture
        // concorrenti mentre è in corso una scrittura, riducendo il rischio di
        // "database is locked" quando più richieste arrivano insieme.
        private static void EnableSqliteWalMode(string xpoConnectionString)
        {
            var sqliteConnectionString = xpoConnectionString.Replace("XpoProvider=SQLite;", string.Empty);
            if (!sqliteConnectionString.Contains("Data Source", StringComparison.OrdinalIgnoreCase))
            {
                return; // Non è SQLite (es. altro provider XPO): niente da fare
            }

            using var connection = new Microsoft.Data.Sqlite.SqliteConnection(sqliteConnectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA journal_mode=WAL;";
            command.ExecuteNonQuery();
        }
    }
}
