using Microsoft.AspNetCore.Mvc;
using Microsoft.CognitiveServices.Speech;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.TextToAudio;
using WordFlux.ApiService;
using WordFlux.ApiService.Ai;
using WordFlux.ApiService.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();
builder.Services.AddOutputCache();

builder.Services.AddCors(cors =>
{
    cors.AddDefaultPolicy(c => c.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

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
app.UseCors();
app.UseSwagger();
app.UseSwaggerUI();
app.UseOutputCache();
app.UseExceptionHandler();

app
    .MapAudioEndpoints()
    .MapCardsEndpoints()
    .MapMotivationalEndpoints()
    .MapTranslationEndpoints()
    ;

app.MapDefaultEndpoints();

app.Run();

