using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using GestionaleRendicontazione.Client;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// ---------------------------------------------------------------------
// HttpClient verso il backend — la base URL viene letta da
// wwwroot/appsettings.json / appsettings.Development.json (chiave "ApiBaseUrl").
// In Fase F1 non viene ancora allegato alcun token: l'autenticazione
// (DelegatingHandler + refresh) arriva in Fase F2.
// ---------------------------------------------------------------------
var apiBaseUrl = builder.Configuration["ApiBaseUrl"]
    ?? throw new InvalidOperationException(
        "Configurazione mancante: valorizzare 'ApiBaseUrl' in wwwroot/appsettings.json.");

builder.Services.AddScoped(_ => new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl)
});

// ---------------------------------------------------------------------
// Blazorise — provider Bootstrap 5 + set di icone FontAwesome
// ---------------------------------------------------------------------
builder.Services
    .AddBlazorise(options =>
    {
        options.Immediate = true;
    })
    .AddBootstrap5Providers()
    .AddFontAwesomeIcons();

await builder.Build().RunAsync();
