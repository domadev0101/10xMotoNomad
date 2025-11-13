using MotoNomad.App;
using MotoNomad.App.Infrastructure.Configuration;
using MotoNomad.App.Infrastructure.Services;
using MotoNomad.App.Infrastructure.Auth;
using MotoNomad.App.Application.Interfaces;
using MotoNomad.Infrastructure.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
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

// Load MockAuth configuration from appsettings.json
var mockAuthSettings = new MockAuthSettings();
builder.Configuration.GetSection("MockAuth").Bind(mockAuthSettings);

// Load OpenRouter configuration from appsettings.json
builder.Services.Configure<OpenRouterSettings>(
    builder.Configuration.GetSection(OpenRouterSettings.SectionName));

// Register Blazored LocalStorage (MUST be before services that use it)
builder.Services.AddBlazoredLocalStorage();

// Register Supabase configuration
builder.Services.AddSingleton(supabaseSettings);
builder.Services.AddSingleton(mockAuthSettings);

// Register Supabase client service as Singleton
builder.Services.AddSingleton<ISupabaseClientService, SupabaseClientService>();

// Register Authentication - use Mock if enabled, otherwise use real Supabase auth
if (mockAuthSettings.Enabled)
{
    builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    {
        var supabaseClient = sp.GetRequiredService<ISupabaseClientService>();
        return new MockAuthenticationStateProvider(
         supabaseClient,
            mockAuthSettings.UserId,
          mockAuthSettings.Email,
   mockAuthSettings.DisplayName);
    });

    // Log warning about mock auth being enabled
    var loggerFactory = builder.Services.BuildServiceProvider().GetRequiredService<ILoggerFactory>();
    var mockLogger = loggerFactory.CreateLogger("Program");
    mockLogger.LogWarning("⚠️ MOCK AUTHENTICATION ENABLED ⚠️");
    mockLogger.LogWarning("Mock User: {Email} (ID: {UserId})", mockAuthSettings.Email, mockAuthSettings.UserId);
    mockLogger.LogWarning("⚠️ This should NEVER be enabled in production!");
}
else
{
    builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
}

builder.Services.AddAuthorizationCore();

// Register application services as Scoped
builder.Services.AddScoped<MotoNomad.Application.Interfaces.IAuthService, AuthService>();
builder.Services.AddScoped<MotoNomad.Application.Interfaces.ITripService, TripService>();
builder.Services.AddScoped<MotoNomad.Application.Interfaces.ICompanionService, CompanionService>();
builder.Services.AddScoped<MotoNomad.Application.Interfaces.IProfileService, ProfileService>();

// Register HttpClient for OpenRouter
builder.Services.AddHttpClient<IOpenRouterService, OpenRouterService>();

// Register OpenRouter service as Scoped
builder.Services.AddScoped<IOpenRouterService, OpenRouterService>();

// Register AI Trip Planner service as Scoped
builder.Services.AddScoped<IAiTripPlannerService, AiTripPlannerService>();

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
