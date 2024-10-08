using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CognitiveServices.Speech;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.TextToAudio;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using WebPush;
using WordFlux.ApiService;
using WordFlux.ApiService.Ai;
using WordFlux.ApiService.Endpoints;
using WordFlux.ApiService.Jobs;
using WordFlux.ApiService.Persistence;
using static System.Net.WebRequestMethods;

var startedDateTime = DateTime.UtcNow;

var builder = WebApplication.CreateBuilder(args);

/*builder.Host.UseSerilog((context, provider, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration);
    //configuration.WriteTo.Console();
    //configuration.WriteTo.Seq("http://172.191.101.172:80");

    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .CreateBootstrapLogger();
});*/

builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;
});

var otel = builder.Services.AddOpenTelemetry();

// Add Metrics for ASP.NET Core and our custom metrics and export via OTLP
otel.WithMetrics(metrics =>
{
    // Metrics provider from OpenTelemetry
    metrics.AddAspNetCoreInstrumentation();
    //Our custom metrics
    // Metrics provides by ASP.NET Core in .NET 8
    metrics.AddMeter("Microsoft.AspNetCore.Hosting");
    metrics.AddMeter("Microsoft.AspNetCore.Server.Kestrel");
});

// Add Tracing for ASP.NET Core and our custom ActivitySource and export via OTLP
otel.WithTracing(tracing =>
{
    tracing.AddSource("Sample.DistributedTracing");
    
    tracing.AddAspNetCoreInstrumentation();
    tracing.AddHttpClientInstrumentation();
    tracing.AddEntityFrameworkCoreInstrumentation();
});

// Export OpenTelemetry data via OTLP, using env vars for the configuration
var OtlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
if (OtlpEndpoint != null)
{
    otel.UseOtlpExporter();
}


/*builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("MyMainApi"))
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation(c =>
            {
                c.Filter = context =>
                {
                    Log.Logger.Information("Filtering trace");

                    return true;
                };
            })
            .AddHttpClientInstrumentation()
            .AddConsoleExporter();
        /*.AddOtlpExporter(options =>
        {
            var seqApiKey = builder.Configuration["OtelApiKey"];
            var endpoint = new Uri(builder.Configuration["OtlpEndpoint"]!);

            Log.Logger.Information("Endpoint is {UtelEndpointUrl}", endpoint);
            Log.Logger.Information("Endpoint is {UtelseqApiKey}", seqApiKey);
            options.Endpoint = endpoint;
            options.Protocol = OtlpExportProtocol.HttpProtobuf;
            options.Headers = $"X-Seq-ApiKey={seqApiKey}";
        });#1#
    });*/

builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<AppUser>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddOptions<BearerTokenOptions>(IdentityConstants.BearerScheme).Configure(options => { options.BearerTokenExpiration = TimeSpan.FromDays(7); });

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

builder.Services.AddHostedService<MigrationHostedService>();

builder.Services.AddSingleton<NotificationsStore>();
builder.Services.AddSingleton<BingImageSearchService>();
builder.Services.AddSingleton<UnsplashImageSearchService>();
builder.Services.AddHostedService<TestBackgroundService>();

builder.Services.AddChannels();
//builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddSwagger();
builder.Services.AddEndpointsApiExplorer();

var dbConnection = builder.Configuration.GetConnectionString("postgresdb");
Console.WriteLine($"Connection string is (console) {dbConnection}");
Log.Information("Connection string is {dbConnection}", dbConnection);

builder.Services.AddNpgsql<ApplicationDbContext>(dbConnection);
builder.Services.AddProblemDetails();
builder.Services.AddOpenAi(builder.Configuration);

var app = builder.Build();
app.UseCors("wasm");
app.UseSwagger();
app.UseSwaggerUI();

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

app.MapPost("/logout", async (SignInManager<AppUser> signInManager, [FromBody] object empty) =>
{
    if (empty is not null)
    {
        await signInManager.SignOutAsync();

        return Results.Ok();
    }

    return Results.Unauthorized();
}).RequireAuthorization();

app.MapGet("/roles", (ClaimsPrincipal user) =>
{
    if (user.Identity is not null && user.Identity.IsAuthenticated)
    {
        var identity = (ClaimsIdentity)user.Identity;
        var roles = identity.FindAll(identity.RoleClaimType)
            .Select(c =>
                new
                {
                    c.Issuer,
                    c.OriginalIssuer,
                    c.Type,
                    c.Value,
                    c.ValueType
                });

        return TypedResults.Json(roles);
    }

    return Results.Unauthorized();
}).RequireAuthorization();

app.MapGet("images/unsplash", async (UnsplashImageSearchService searchService, string keyword) => { return await searchService.GetImagesByKeyword(keyword); });

app.MapGet("images", async (BingImageSearchService searchService, UnsplashImageSearchService unsplashSearch, string keyword, bool isUnsplash = true) =>
    {
        if (isUnsplash)
        {
            return await unsplashSearch.GetImagesByKeyword(keyword);
        }

        return await searchService.GetImagesByKeyword(keyword);
    })
    .CacheOutput(p =>
    {
        p.AddPolicy<OutputCachePolicy>();
        p.Expire(TimeSpan.FromHours(2))
            .SetVaryByQuery("keyword");
    });

app
    .MapAudioEndpoints()
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
