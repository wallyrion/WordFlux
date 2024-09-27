using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CognitiveServices.Speech;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.TextToAudio;
using Serilog;
using WebPush;
using WordFlux.ApiService;
using WordFlux.ApiService.Ai;
using WordFlux.ApiService.Endpoints;
using WordFlux.ApiService.Persistence;
using static System.Net.WebRequestMethods;

var startedDateTime = DateTime.UtcNow;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, provider, configuration) =>
{
    configuration.WriteTo.Console();
    configuration.WriteTo.Seq("http://172.191.101.172:80");
});
builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<AppUser>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddOptions<BearerTokenOptions>(IdentityConstants.BearerScheme).Configure(options =>
{
    options.BearerTokenExpiration = TimeSpan.FromDays(7);
});

// Add service defaults & Aspire components.
//builder.AddServiceDefaults();

builder.Services.AddOutputCache();
builder.Services.AddCors(
    options => options.AddPolicy(
        "wasm",
        policy => policy
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithOrigins("https://localhost:7153", "https://wordflux.online", "https://icy-ocean-03f34ba03-13.westeurope.5.azurestaticapps.net")));


if (builder.Configuration["UseAzureKeyVault"] == "true")
{
    //Console.WriteLine("Using Azure Key Vault");
    //var secretsUrl = builder.Configuration["Secrets:Url"];

    //builder.Configuration.AddAzureKeyVault(new Uri(secretsUrl!), new ClientSecretCredential(secrets.TenantId, secrets.ClientId, secrets.ClientSecret));

    //builder.Configuration.AddAzureKeyVaultSecrets("secrets");
}

builder.Services.AddSingleton<NotificationsStore>();
builder.Services.AddSingleton<BingImageSearchService>();
builder.Services.AddSingleton<UnsplashImageSearchService>();
builder.Services.AddHostedService<TestBackgroundService>();
// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddSwagger();
builder.Services.AddEndpointsApiExplorer();
builder.AddNpgsqlDbContext<ApplicationDbContext>("postgresdb");
builder.Services.AddHostedService<MigrationHostedService>();

builder.Services.AddOpenAi(builder.Configuration);

var app = builder.Build();
app.UseCors("wasm");
app.UseSwagger();
app.UseSwaggerUI();
app.UseSerilogRequestLogging();
app.UseExceptionHandler();

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


app.MapGet("images/unsplash", async (UnsplashImageSearchService searchService, string keyword) =>
{

    return await searchService.GetImagesByKeyword(keyword);
    

});

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


app.MapGet("/health", (IConfiguration configuration) => new
{
    ImageTag = configuration["CurrentImageTag"],
    StartedDate = startedDateTime,
    AliveTime = DateTime.UtcNow - startedDateTime
});


app.MapDefaultEndpoints();

app.Run();