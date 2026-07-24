using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using GestionaleRendicontazione.Client;
using GestionaleRendicontazione.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["ApiBaseUrl"]
    ?? throw new InvalidOperationException(
        "Configurazione mancante: valorizzare 'ApiBaseUrl' in wwwroot/appsettings.json.");

builder.Services.AddScoped<TokenStorageService>();
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthenticationStateProvider>());
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<WorkLogApiClient>();
builder.Services.AddScoped<CompanyApiClient>();
builder.Services.AddScoped<ProjectApiClient>();
builder.Services.AddScoped<EmployeeApiClient>();
builder.Services.AddScoped<StatusApiClient>();
builder.Services.AddScoped<TypeApiClient>();
builder.Services.AddScoped<ReportDataApiClient>();
builder.Services.AddScoped<FilterStateService>();
builder.Services.AddScoped<LogApiClient>();
builder.Services.AddScoped<GestionaleRendicontazione.Client.Services.Reports.PdfReportService>();
builder.Services.AddScoped<GestionaleRendicontazione.Client.Services.Reports.DownloadInterop>();
builder.Services.AddTransient<AuthenticatedHttpMessageHandler>();

builder.Services.AddScoped(sp =>
{
    var handler = sp.GetRequiredService<AuthenticatedHttpMessageHandler>();
    handler.InnerHandler = new HttpClientHandler();
    return new HttpClient(handler)
    {
        BaseAddress = new Uri(apiBaseUrl)
    };
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
