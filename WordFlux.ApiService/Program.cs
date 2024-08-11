using Microsoft.EntityFrameworkCore;
using TwitPoster.Web.WebHostServices;
using WordFlux.ApiService;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

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
app.UseSwagger();
app.UseSwaggerUI();


// Configure the HTTP request pipeline.
app.UseExceptionHandler();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/cards", async (ApplicationDbContext dbContext, ILogger<Program> logger, Guid userId) =>
{
    return await dbContext.Cards.Where(c => c.CreatedBy == userId).ToListAsync();
});

app.MapPost("/cards", async (ApplicationDbContext dbContext, ILogger<Program> logger, CardRequest request, Guid userId) =>
{
    var card = new Card
    {
        CreatedAt = DateTime.UtcNow,
        Id= Guid.NewGuid(),
        Term = request.Term,
        Translations = request.Translations,
        CreatedBy = userId
    };
    dbContext.Cards.Add(card);
    await dbContext.SaveChangesAsync();
});

#pragma warning disable SKEXP0010

app.MapGet("/term", async (ApplicationDbContext dbContext, ILogger<Program> logger, string term, OpenAiGenerator translation) =>
{
    var respose = await translation.GetTranslations(term);

    return respose;
    /*KernelArguments arguments = new(new OpenAIPromptExecutionSettings
    {

    }) { { "term", term } };

    var result = await kernel.InvokePromptAsync(Examples.RequestForAssistantWithArguments, arguments);

    var strValue = result.GetValue<string>();

    if (strValue.StartsWith("```json"))
    {
        strValue = strValue[7..^3];
    }

    var translationResult = JsonSerializer.Deserialize<TranslationResult>(strValue);

    var response = new TranslationResponse(translationResult.Term, translationResult.Translations.Select(t => new TranslationItem(t.Term, t.ExampleTranslated, t.ExampleOriginal)));

    return response;*/
});
#pragma warning restore SKEXP0010


app.MapDefaultEndpoints();

app.Run();





