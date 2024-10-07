using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using WordFlux.Contracts;

namespace WordFlux.ApiService.Ai;

public class OpenAiGenerator : IOpenAiGenerator
{
    private readonly Kernel _kernel;
    private readonly ILogger<OpenAiGenerator> _logger;

    public OpenAiGenerator(Kernel kernel, ILogger<OpenAiGenerator> logger)
    {
        _kernel = kernel;
        _logger = logger;
    }

    
    [Experimental("SKEXP0010")]
    public async Task<(string detectedLanguage, List<(string, string)> autocompletes)?> GetAutocompleteWithTranslations(string term, string lang1, string lang2, CancellationToken cancellationToken = default)
    {
        KernelArguments arguments = new(new OpenAIPromptExecutionSettings
        {
            ResponseFormat = "json_object",
            Temperature = 1
        }) { { "lang1", lang1 }, { "lang2", lang2 }, { "term", term } };

        
        var result = await AiFunctions.AutocompleteWithTranslationFunc.InvokeAsync<OpenAIChatMessageContent>(_kernel, arguments, cancellationToken);
        
        if (result == null || result.Content == null)
        {
            _logger.LogError("Got null result");

            return null;
        }

        var content = JsonSerializer.Deserialize<AutocompleteWithTranslationsResult>(result.Content);

        if (content == null)
        {
            return null;
        }

        return (content.DetectedLanguage, content.Autocompletes.Select(x => (x.AutocompleteResult, x.TranslatedAutocompleteResult)).ToList());
    }
    [Experimental("SKEXP0010")]
    public async Task<(string detectedLanguage, List<string> autocompletes)?> GetAutocomplete(string term, string lang1, string lang2)
    {
        KernelArguments arguments = new(new OpenAIPromptExecutionSettings
        {
            ResponseFormat = "json_object",
            Temperature = 1
        }) { { "lang1", lang1 }, { "lang2", lang2 }, { "term", term } };

        var result = await AiFunctions.AutocompleteFunc.InvokeAsync<OpenAIChatMessageContent>(_kernel, arguments);

        if (result == null || result.Content == null)
        {
            _logger.LogError("Got null result");

            return null;
        }

        var content = JsonSerializer.Deserialize<AutocompleteResult>(result.Content);

        if (content == null)
        {
            return null;
        }

        return (content.DetectedLanguage, content.Autocompletes);
    }
    
    [Experimental("SKEXP0010")]
    public async Task<(string srcLang, string destLang)?> DetectLanguages(string src, string dest)
    {
        KernelArguments arguments = new(new OpenAIPromptExecutionSettings
        {
            ResponseFormat = "json_object",
            Temperature = 0.5
        }) { { "src", src }, { "dest", dest } };

        var result = await AiFunctions.DetectLanguagesFunc.InvokeAsync<OpenAIChatMessageContent>(_kernel, arguments);

        if (result == null || result.Content == null)
        {
            _logger.LogError("Got null result");

            return null;
        }

        var content = JsonSerializer.Deserialize<DetectLanguagesResponse>(result.Content);

        if (content == null)
        {
            return null;
        }

        return (content.SourceLanguage, content.DestinationLanguage);
    }

    [Experimental("SKEXP0010")]
    public async Task<(string sourceLanguage, string? suggestedTerm)?> DetectLanguage(string input, List<string> possibleLanguages)
    {
        KernelArguments arguments = new(new OpenAIPromptExecutionSettings
        {
            ResponseFormat = "json_object",
            Temperature = 0.5
        }) { { "input", input }, { "possibleLanguages", string.Join(",", possibleLanguages) } };

        var result = await AiFunctions.DetectLanguageFunc.InvokeAsync<OpenAIChatMessageContent>(_kernel, arguments);

        if (result?.Content == null)
        {
            _logger.LogError("Got null result");

            return null;
        }

        var content = JsonSerializer.Deserialize<DetectLanguageResponse>(result.Content);

        return (content!.Language, content.SuggestedTerm);
    }
    
    [Experimental("SKEXP0010")]
    public async Task<(string sourceLanguage, string destinationLanguage)?> DetectLanguage(string sourceInput, string translatedInput, CancellationToken cancellationToken = default)
    {
        KernelArguments arguments = new(new OpenAIPromptExecutionSettings
        {
            ResponseFormat = "json_object",
            Temperature = 0.5
        }) { { "src", sourceInput }, { "dest", translatedInput }  };

        var result = await AiFunctions.DetectPossibleLanguageFunc.InvokeAsync<OpenAIChatMessageContent>(_kernel, arguments, cancellationToken);

        if (result?.Content == null)
        {
            _logger.LogError("Got null result");

            return null;
        }

        var content = JsonSerializer.Deserialize<DetectMultipleLanguagesResponse>(result.Content);

        return (content.SourceLanguage, content.DestinationLanguage);
    }

    [Experimental("SKEXP0010")]
    public async Task<List<TranslationItem>> GetExamples(string term, List<string> translations, string sourceLanguage, string destinationLanguage)
    {
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

        var content = JsonSerializer.Deserialize<TranslationExampleResult>(result.Content);

        if (content == null)
        {
            return [];
        }

        return content.Translations
            .Select(x =>
                new TranslationItem(x.Term, x.ExampleTranslated, x.ExampleOriginal, 0, x.Level))
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
    public async Task<SimpleTranslationResponse?> GetTranslations(string term, string inputLanguage, string translationsLanguage, int translationsCount = 4)
    {
        KernelArguments arguments = new(new OpenAIPromptExecutionSettings
        {
            ResponseFormat = "json_object",
            Temperature = 1
        }) { { "term", term }, { "inputLang", inputLanguage }, { "translationsLang",translationsLanguage}, { "translationsCount", translationsCount  }  };

        var result = await AiFunctions.TranslationsFunc.InvokeAsync<OpenAIChatMessageContent>(_kernel, arguments);

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
    public async Task<SimpleTranslationResponse?> GetAlternativeTranslations(string term, string sourceLanguage, string destinationLanguage,
        IEnumerable<string> translations)
    {
        KernelArguments arguments = new(new OpenAIPromptExecutionSettings
        {
            ResponseFormat = "json_object",
            Temperature = 0.5
        })
        {
            { "term", term }, { "srcLang", sourceLanguage }, { "destLang", destinationLanguage },
            { "existingTranslations", JsonSerializer.Serialize(translations) }
        };

        var result = await AiFunctions.GiveAlternativesFunc.InvokeAsync<OpenAIChatMessageContent>(_kernel, arguments);

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
    public async Task<List<(string ExampleLearn, string ExampleNative)>?> GetExamplesCardTask(string term, string learnLanguage, string nativeLanguage, int examplesCount, IEnumerable<string> translations, CancellationToken cancellationToken = default)
    {
        KernelArguments arguments = new(new OpenAIPromptExecutionSettings
        {
            ResponseFormat = "json_object",
            Temperature = 1
        })
        {
            { "term", term }, { "learnLang", learnLanguage }, { "nativeLang", nativeLanguage }, { "count", examplesCount },{ "translations", JsonSerializer.Serialize(translations) },
        };

        var result = await AiFunctions.CreateCardExampleTaskFunc.InvokeAsync<OpenAIChatMessageContent>(_kernel, arguments, cancellationToken);

        if (result?.Content == null)
        {
            return [];
        }

        var content = JsonSerializer.Deserialize<CardExampleTaskResult>(result.Content);

        if (content == null)
        {
            return [];
        }

        return content.Examples.Select(x => (x.ExampleToLearn, x.ExampleOriginal)).ToList();
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
    [JsonPropertyName("tr")] public string Term { get; set; }

    [JsonPropertyName("e_tr")] public string ExampleTranslated { get; set; }

    [JsonPropertyName("e_or")] public string ExampleOriginal { get; set; }

    [JsonPropertyName("p")] public string Popularity { get; set; }

    [JsonPropertyName("l")] public string Level { get; set; }
}

file class TranslationExampleResult
{
    [JsonPropertyName("translations")] public List<TranslationItemDtoNew> Translations { get; set; }
}

file class DetectLanguagesResponse
{
    [JsonPropertyName("srcLang")] public string SourceLanguage { get; set; } = null!;

    [JsonPropertyName("destLang")] public string DestinationLanguage { get; set; } = null!;
}

file class DetectLanguageResponse
{
    [JsonPropertyName("language")] public string Language { get; set; } = null!;
    [JsonPropertyName("suggested_term")] public string? SuggestedTerm { get; set; }
}

file class DetectMultipleLanguagesResponse
{
    [JsonPropertyName("srcLanguage")] public required string SourceLanguage { get; set; } 
    [JsonPropertyName("destLanguage")] public required string DestinationLanguage { get; set; }
}


file class EstimateLevelResult
{
    [JsonPropertyName("level")] public string Level { get; set; } = null!;
}

file class TranslationResultNew
{
    [JsonPropertyName("translations")] public List<string> Translations { get; set; }

    [JsonPropertyName("suggested_term")] public string? SuggestedTerm { get; set; }

    [JsonPropertyName("srcL")] public string SourceLanguage { get; set; } = null!;

    [JsonPropertyName("outL")] public string OutputLanguage { get; set; } = null!;
}

file class CardExampleTaskResult
{
    [JsonPropertyName("sentences")] public List<CardExampleTaskItem> Examples { get; set; } = [];
}

file class CardExampleTaskItem
{
    [JsonPropertyName("example_original")] public required string ExampleToLearn { get; set; }
    [JsonPropertyName("example_translated")] public required string ExampleOriginal { get; set; }
}

file class AutocompleteWithTranslationsResult
{
    [JsonPropertyName("autocompletes")] public List<AutocompleteTranslationItem> Autocompletes { get; set; }
    [JsonPropertyName("lang")] public string DetectedLanguage { get; set; }

  
}

file class AutocompleteResult
{
    [JsonPropertyName("autocompletes")] public List<string> Autocompletes { get; set; }
    [JsonPropertyName("lang")] public string DetectedLanguage { get; set; }
  
}

file class AutocompleteTranslationItem
{
    [JsonPropertyName("term")] 
    public string AutocompleteResult { get; set; } 
        
    [JsonPropertyName("term_translated")] 
    public string TranslatedAutocompleteResult { get; set; } 
}