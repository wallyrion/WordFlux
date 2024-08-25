using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using WordFlux.Contracts;

namespace WordFlux.ApiService.Ai;

public class OpenAiGenerator
{
    private readonly Kernel _kernel;
    private readonly ILogger<OpenAiGenerator> _logger;

    public OpenAiGenerator(Kernel kernel, ILogger<OpenAiGenerator> logger)
    {
        _kernel = kernel;
        _logger = logger;
    }

    [Experimental("SKEXP0010")]
    public async Task<(string srcLang, string destLang)?> DetectLanguage(string src, string dest)
    {
        KernelArguments arguments = new(new OpenAIPromptExecutionSettings
        {
            ResponseFormat = "json_object",
            Temperature = 0.5
        }) { { "src", src }, { "dest", dest }};
        
        var result = await AiFunctions.DetectLanguageFunc.InvokeAsync<OpenAIChatMessageContent>(_kernel, arguments);

        if (result == null || result.Content == null)
        {
            _logger.LogError("Got null result");

            return null;
        }
        
        _logger.LogInformation("Got result {result}", result.Content);

        var content = JsonSerializer.Deserialize<DetectLanguageResponse>(result.Content);

        if (content == null)
        {
            return null;
        }

        return (content.SourceLanguage, content.DestinationLanguage);
    }
    
    [Experimental("SKEXP0010")]
    public async Task<List<TranslationItem>> GetExamples(string term, List<string> translations, string sourceLanguage, string destinationLanguage)
    {
        _logger.LogInformation("Getting examples for term {Term} and translations {@Translations}", term, translations);
        
        KernelArguments arguments = new(new OpenAIPromptExecutionSettings
        {
            ResponseFormat = "json_object",
            Temperature = 0.5
        }) { { "term", term }, { "srcLang", sourceLanguage }, { "destLang", destinationLanguage }, { "translations", JsonSerializer.Serialize(translations) } };
        
        var result = await AiFunctions.GiveExamplesFunc.InvokeAsync<OpenAIChatMessageContent>(_kernel, arguments);

        if (result == null || result.Content == null)
        {
            _logger.LogError("Got null result");

            return [];
        }
        
        _logger.LogInformation("Got result {result}", result.Content);

        var content = JsonSerializer.Deserialize<TranslationExampleResult>(result.Content);

        if (content == null)
        {
            return [];
        }

        return content.Translations
            .Select(x =>
                new TranslationItem(x.Term, x.ExampleTranslated, x.ExampleOriginal, int.Parse(x.Popularity), x.Level))
            .ToList();
    }
    

    [Experimental("SKEXP0010")]
    public async Task<string?> GetLevel(string term)
    {
        KernelArguments arguments = new(new OpenAIPromptExecutionSettings
        {
            ResponseFormat = "json_object"
        }) { { "term", term } };
        
        var result = await AiFunctions.GetLevelFunc.InvokeAsync<OpenAIChatMessageContent>(_kernel, arguments);

        if (result == null || result.Content == null)
        {
            return null;
        }

        var levelResult = JsonSerializer.Deserialize<EstimateLevelResult>(result.Content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (levelResult == null)
        {
            return null;
        }

        return levelResult.Level;
    }
    
    
    public async Task<string?> GetMotivationalPhrase()
    {
        var result = await AiFunctions.GiveMotivationalPhraseFunc.InvokeAsync<OpenAIChatMessageContent>(_kernel);

        return result?.Content;
    }
    
    [Experimental("SKEXP0010")]
    public async Task<SimpleTranslationResponse?> GetTranslations(string term, List<string> languages)
    {
        _logger.LogInformation("Getting translations for term {Term}", term);
        
        KernelArguments arguments = new(new OpenAIPromptExecutionSettings
        {
            ResponseFormat = "json_object",
            Temperature = 0.5
        }) { { "term", term }, { "languages", JsonSerializer.Serialize(languages) } };
        
        var result = await AiFunctions.TranslationsFunc.InvokeAsync<OpenAIChatMessageContent>(_kernel, arguments);

        _logger.LogInformation("Got translated results {result}", result.Content);
        
        if (result == null || result.Content == null)
        {
            return null;
        }

        var content = JsonSerializer.Deserialize<TranslationResultNew>(result.Content);

        if (content == null)
        {
            return null;
        }

        var response = new SimpleTranslationResponse(content.SuggestedTerm, content.Translations, content.SourceLanguage, content.OutputLanguage);

        return response;
    }

    [Experimental("SKEXP0010")]
    public async Task<SimpleTranslationResponse?> GetAlternativeTranslations(string term, string sourceLanguage, string destinationLanguage, IEnumerable<string> translations)
    {
        _logger.LogInformation("Getting alternative translations for term {Term}", term);
        
        KernelArguments arguments = new(new OpenAIPromptExecutionSettings
        {
            ResponseFormat = "json_object",
            Temperature = 0.5
        }) { { "term", term }, { "srcLang", sourceLanguage }, { "destLang", destinationLanguage }, { "existingTranslations", JsonSerializer.Serialize(translations) } };
        
        var result = await AiFunctions.GiveAlternativesFunc.InvokeAsync<OpenAIChatMessageContent>(_kernel, arguments);
        
        _logger.LogInformation("Got translated results {result}", result.Content);
        
        if (result == null || result.Content == null)
        {
            return null;
        }

        var content = JsonSerializer.Deserialize<TranslationResultNew>(result.Content);

        if (content == null)
        {
            return null;
        }

        var response = new SimpleTranslationResponse(content.SuggestedTerm, content.Translations, content.SourceLanguage, content.OutputLanguage);

        return response;
    }
}
    
/*file class TranslationResult
{
    [JsonPropertyName("term")]
    public string Term { get; set; }
    
    [JsonPropertyName("list")]
    public List<TranslationItemDto> Translations { get; set; }
    
    [JsonPropertyName("suggestedTerm")]
    public string? SuggestedTerm { get; set; }
        
    [JsonPropertyName("l")]
    public string Level { get; set; }
}*/

/*file class TranslationItemDto
{
    [JsonPropertyName("tr")]
    public string Term { get; set; }
    
    [JsonPropertyName("e_tr")]
    public string ExampleTranslated { get; set; }

    [JsonPropertyName("e_or")]
    public string ExampleOriginal { get; set; }
    
    [JsonPropertyName("u_f")]
    public int Popularity { get; set; }
        
    [JsonPropertyName("l")]
    public string Level { get; set; }
}*/

public class TranslationItemDtoNew
{
    [JsonPropertyName("tr")]
    public string Term { get; set; }
    
    [JsonPropertyName("e_tr")]
    public string ExampleTranslated { get; set; }

    [JsonPropertyName("e_or")]
    public string ExampleOriginal { get; set; }
    
    [JsonPropertyName("p")]
    public string Popularity { get; set; }
        
    [JsonPropertyName("l")]
    public string Level { get; set; }
}


file class TranslationExampleResult
{
    [JsonPropertyName("translations")]
    public List<TranslationItemDtoNew> Translations { get; set; }
}

file class DetectLanguageResponse
{
    [JsonPropertyName("srcLang")]
    public string SourceLanguage { get; set; } = null!;

    [JsonPropertyName("destLang")] public string DestinationLanguage { get; set; } = null!;
}


file class EstimateLevelResult
{
    [JsonPropertyName("level")] 
    public string Level { get; set; } = null!;
}

file class TranslationResultNew
{
    [JsonPropertyName("translations")]
    public List<string> Translations { get; set; }
    
    [JsonPropertyName("suggested_term")]
    public string? SuggestedTerm { get; set; }


    [JsonPropertyName("srcL")] 
    public string SourceLanguage { get; set; } = null!;
    
        
    [JsonPropertyName("outL")]
    public string OutputLanguage { get; set; } = null!;
}
