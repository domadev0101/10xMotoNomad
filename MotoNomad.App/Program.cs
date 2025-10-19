using MotoNomad.App;
using MotoNomad.App.Infrastructure.Configuration;
using MotoNomad.App.Infrastructure.Services;
using MotoNomad.App.Application.Interfaces;
using MotoNomad.Infrastructure.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Blazored.LocalStorage;
using Microsoft.Extensions.Configuration;
using MudBlazor.Services;

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

// Register application services as Scoped
builder.Services.AddScoped<MotoNomad.Application.Interfaces.IAuthService, AuthService>();
builder.Services.AddScoped<MotoNomad.Application.Interfaces.ITripService, TripService>();
builder.Services.AddScoped<MotoNomad.Application.Interfaces.ICompanionService, CompanionService>();
builder.Services.AddScoped<MotoNomad.Application.Interfaces.IProfileService, ProfileService>();

// Register Blazored LocalStorage
builder.Services.AddBlazoredLocalStorage();

// Register MudBlazor services
builder.Services.AddMudServices();

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
