using MotoNomad.App;
using MotoNomad.App.Infrastructure.Configuration;
using MotoNomad.App.Infrastructure.Services;
using MotoNomad.App.Application.Interfaces;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Blazored.LocalStorage;
using Microsoft.Extensions.Configuration;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HttpClient
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Load Supabase configuration from appsettings.json
var supabaseSettings = new SupabaseSettings();
builder.Configuration.GetSection("Supabase").Bind(supabaseSettings);

// Register Supabase configuration
builder.Services.AddSingleton(supabaseSettings);

// Register Supabase client service as Singleton
builder.Services.AddSingleton<ISupabaseClientService, SupabaseClientService>();

// Register Blazored LocalStorage
builder.Services.AddBlazoredLocalStorage();

// Build the application
var app = builder.Build();

// Initialize Supabase client before running the app
var logger = app.Services.GetRequiredService<ILogger<Program>>();
try
{
    logger.LogInformation("Initializing Supabase connection...");
    var supabaseClient = app.Services.GetRequiredService<ISupabaseClientService>();
    await supabaseClient.InitializeAsync();
    logger.LogInformation("Application startup completed successfully");
}
catch (Exception ex)
{
    logger.LogError(ex, "Failed to initialize Supabase during startup");
    throw;
}

await app.RunAsync();
