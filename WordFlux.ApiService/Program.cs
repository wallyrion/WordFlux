using System.Diagnostics;
using System.Reflection;
using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WordFlux.ApiService.Endpoints;
using WordFlux.ApiService.ServiceCollectionExtensions;
using WordFlux.Application.Common;
using WordFlux.Application.Common.Behaviours;
using WordFlux.Application.Jobs;
using WordFlux.Domain;
using WordFlux.Infrastructure;
using WordFlux.Infrastructure.Authorization;
using WordFlux.Infrastructure.ImageSearch;
using WordFlux.Infrastructure.Messaging.Consumers;
using WordFlux.Infrastructure.Observability;
using WordFlux.Infrastructure.Persistence;
using WordFlux.Translations.Ai;


var startedDateTime = DateTime.UtcNow;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWordfluxHttpLogging();
builder.Logging.AddLogging(builder.Configuration);
builder.Services.AddTelemetry(builder.Configuration);

builder.Services.AddServiceDiscovery();
builder.Services.ConfigureHttpClientDefaults(http =>
{
    // Turn on resilience by default
    http.AddStandardResilienceHandler();

    // Turn on service discovery by default
    http.AddServiceDiscovery();
});

builder.Services.AddScoped<IDbContext, ApplicationDbContext>(x => x.GetRequiredService<ApplicationDbContext>());

// Add service defaults & Aspire components.
//builder.AddServiceDefaults();

builder.Services.AddOutputCache();
builder.Services.AddCors(
    options => options.AddPolicy(
        "wasm",
        policy => policy
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithOrigins("https://localhost:7153", "https://wordflux.online", "https://www.wordflux.online", "https://icy-ocean-03f34ba03-13.westeurope.5.azurestaticapps.net")));

if (builder.Configuration["UseAzureKeyVault"] == "true")
{
    //Console.WriteLine("Using Azure Key Vault");
    //var secretsUrl = builder.Configuration["Secrets:Url"];

    //builder.Configuration.AddAzureKeyVault(new Uri(secretsUrl!), new ClientSecretCredential(secrets.TenantId, secrets.ClientId, secrets.ClientSecret));

    //builder.Configuration.AddAzureKeyVaultSecrets("secrets");
}

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices();
builder.Services.AddWordfluxAuthorization();
await builder.Services.AddKeysProtectionPersistence(builder.Configuration);

builder.Services.AddSingleton<NotificationsStore>();
builder.Services.AddSingleton<BingImageSearchService>();
builder.Services.AddSingleton<UnsplashImageSearchService>();
builder.Services.AddHostedService<CardsPushNotificationsBackgroundService>();

builder.Services.AddMassTransit(x =>
{
    x.UsingInMemory((context, configurator) => 
    {
        configurator.ConfigureEndpoints(context);
    });

    x.AddConsumer<ImportDeckEventConsumer>();
});
builder.Services.AddChannels();
//builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddSwagger();
builder.Services.AddEndpointsApiExplorer();

var dbConnection = builder.Configuration.GetConnectionString("postgresdb");
Console.WriteLine($"Connection string is (console) {dbConnection}");

builder.Services.AddDbContext<ApplicationDbContext>(c =>
{
    c.UseNpgsql(dbConnection);
    c.EnableDetailedErrors();
    c.EnableSensitiveDataLogging();
});

builder.Services.AddProblemDetails();
builder.Services.AddOpenAi(builder.Configuration);

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseHttpLogging();
app.UseCors("wasm");

app.MapGlobalErrorHandling();
//app.UseExceptionHandler();


app.MapGet("/test", () =>
{
    throw new Exception("eqweqw");
});

app.MapIdentityApi<AppUser>();
app.UseOutputCache();

app.UseAuthentication();
app.UseAuthorization();


app
    .MapAuthEndpoints()
    .MapAudioEndpoints()
    .MapImagesEndpoints()
    .MapCardsEndpoints()
    .MapDecksEndpoints()
    .MapMotivationalEndpoints()
    .MapTranslationEndpoints()
    .MapPushNotificationsEndpoints()
    ;

app.MapGet("/health", async (IConfiguration configuration) =>
{
    var currentActivity = Activity.Current;

    var metadata = new Dictionary<string, object>
    {
        { "activity", currentActivity }
    };
    //await TestDistributedTracesBackgroundJob.MyChannel.Writer.WriteAsync((Guid.NewGuid(), metadata ));

    return new
    {
        ImageTag = configuration["CurrentImageTag"],
        StartedDate = startedDateTime,
        AliveTime = DateTime.UtcNow - startedDateTime
    };
});

//app.MapDefaultEndpoints();

app.Run();
