using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using WordFLux.ClientApp;
using WordFLux.ClientApp.Services;
using WordFLux.ClientApp.Storage;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");


builder.Services.AddScoped<TranslationsSyncService>();
builder.Services.AddTransient<LocalStorage>();
builder.Services.AddBlazoredLocalStorage();

builder.Services.AddScoped<WeatherApiClient>();
builder.Services.AddSingleton<ConnectionHealthManager>();
builder.Services.AddSingleton<InMemoryMessageQueue>();

// https://localhost:7443/
// https://apiservice.jollycliff-5a69ab58.westeurope.azurecontainerapps.io/

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7443/") });

await builder.Build().RunAsync();