using Blazored.LocalStorage;
using BlazorWasmAuth.Identity;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using WordFLux.ClientApp;
using WordFLux.ClientApp.Identity;
using WordFLux.ClientApp.Services;
using WordFLux.ClientApp.Storage;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddTransient<CookieHandler>();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, TokenAuthenticationStateProvider>();
builder.Services.AddScoped<TokenProvider>();
builder.Services.AddTransient<TokenHandler>();
builder.Services.AddScoped(
    sp => (IAccountManagement)sp.GetRequiredService<AuthenticationStateProvider>());

builder.Services.AddScoped<TranslationsSyncService>();
builder.Services.AddTransient<LocalStorage>();
builder.Services.AddBlazoredLocalStorage();

//builder.Services.AddScoped<WeatherApiClient>();
builder.Services.AddSingleton<ConnectionHealthService>();
builder.Services.AddSingleton<InMemoryMessageQueue>();

// https://localhost:7443/
// https://apiservice.jollycliff-5a69ab58.westeurope.azurecontainerapps.io/

var apiUrl = "https://apiservice.jollycliff-5a69ab58.westeurope.azurecontainerapps.io/";

builder.Services.AddHttpClient<WeatherApiClient>((provider, client) =>
    {
        var configuration = provider.GetRequiredService<IConfiguration>();
        var url = configuration["BackendUrl"];
        client.BaseAddress = new Uri(url!);
    })
    .AddHttpMessageHandler<TokenHandler>();

builder.Services.AddHttpClient(
        // need to squash auth http client into default.
        
        "Auth", (provider, client) =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            var url = configuration["BackendUrl"];
            client.BaseAddress = new Uri(url!);
        })
    .AddHttpMessageHandler<TokenHandler>();
await builder.Build().RunAsync();