using AutoMapper;

namespace GestionaleRendicontazione.Api.Helpers
{
    /// <summary>
    /// Diagnostica dell'AutoMapper all'avvio dell'app. Senza questo check
    /// un mapping errato (es. proprietà typo sul DTO) verrebbe scoperto solo
    /// alla prima chiamata API coinvolta, con un'eccezione a runtime.
    ///
    /// <see cref="AddAutoMapperStartupCheck"/> aggiunge all'applicazione un
    /// check che, in caso di mapping configurato in modo non valido, lancia
    /// un'eccezione chiara al boot con l'elenco dei problemi.
    /// </summary>
    public static class AutoMapperDiagnostics
    {
        public static WebApplication AddAutoMapperStartupCheck(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
            mapper.ConfigurationProvider.AssertConfigurationIsValid();
            return app;
        }
    }
}
