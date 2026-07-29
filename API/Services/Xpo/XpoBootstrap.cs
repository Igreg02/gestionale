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

        private static void EnableSqliteWalMode(string xpoConnectionString)
        {
            var sqliteConnectionString = xpoConnectionString.Replace("XpoProvider=SQLite;", string.Empty);
            if (!sqliteConnectionString.Contains("Data Source", StringComparison.OrdinalIgnoreCase))
            {
                return; 
            }

            using var connection = new Microsoft.Data.Sqlite.SqliteConnection(sqliteConnectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA journal_mode=WAL;";
            command.ExecuteNonQuery();
        }
    }
}
