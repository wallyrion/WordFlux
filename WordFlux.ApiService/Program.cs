using System.Security.Claims;
using System.Text;
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
using WordFlux.ApiService;
using WordFlux.ApiService.Ai;
using WordFlux.ApiService.Endpoints;
using WordFlux.ApiService.Persistence;
using static System.Net.WebRequestMethods;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<AppUser>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddOptions<BearerTokenOptions>(IdentityConstants.BearerScheme).Configure(options => {
    options.BearerTokenExpiration = TimeSpan.FromDays(1);
});
//builder.Services.AddAuthorizationBuilder();
/*builder.Services.AddIdentityCore<AppUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddApiEndpoints();*/


/*builder.Services.AddCors(
    options => options.AddPolicy(
        "wasm",
        policy => policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()));*/

// Add service defaults & Aspire components.
builder.AddServiceDefaults();
builder.Services.AddOutputCache();
builder.Services.AddCors(
    options => options.AddPolicy(
    "wasm",
        policy => policy.WithOrigins([builder.Configuration["FrontendUrl"] ?? "https://localhost:7153", "https://delightful-smoke-000aa9910-preview.centralus.5.azurestaticapps.net"])
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()));
/*builder.Services.AddCors(cors =>
{
    cors.AddDefaultPolicy(c => c.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});*/

if (builder.Configuration["UseAzureKeyVault"] == "true")
{
    Console.WriteLine("Using Azure Key Vault");
    builder.Configuration.AddAzureKeyVaultSecrets("secrets");
}

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
app.UseExceptionHandler();

app.MapIdentityApi<AppUser>();

app.UseAuthentication();
app.UseAuthorization();

app.UseOutputCache();

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
app
    .MapAudioEndpoints()
    .MapCardsEndpoints()
    .MapMotivationalEndpoints()
    .MapTranslationEndpoints()
    ;

app.MapDefaultEndpoints();

app.Run();

