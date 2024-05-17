using Receipts_UI.Src;
using MudBlazor.Services;
using Serilog.Events;
using Serilog;
using Microsoft.Extensions.Configuration;
using Serilog.Sinks.Datadog.Logs;
using Azure.Identity;
using System.Net.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();
builder.Services.AddMudServices();

builder.Services.AddScoped<HttpClient>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>(); // Retrieve IConfiguration from service provider
    var baseAddress = new Uri(configuration["ApiBaseAddress"]);
    return new HttpClient { BaseAddress = baseAddress };
});

// Configure Serilog
var datadogApiKey = builder.Configuration["Datadog:ApiKey"];
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.DatadogLogs(
        apiKey: datadogApiKey,
        configuration: new Serilog.Sinks.Datadog.Logs.DatadogConfiguration
        {
            Url = "https://http-intake.logs.datadoghq.com"
        })
    .CreateLogger();

builder.Host.UseSerilog();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Receipts_UI.Src._Imports).Assembly);
app.MapBlazorHub();
app.MapFallbackToPage("/Home");

app.Run();
