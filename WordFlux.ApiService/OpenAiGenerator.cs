﻿using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace WordFlux.ApiService;

public class OpenAiGenerator
{
    private readonly Kernel _kernel;
    private readonly ILogger<OpenAiGenerator> _logger;
    /*private readonly KernelFunction _translationsFunc = KernelFunctionFactory.CreateFromPrompt(new PromptTemplateConfig
    {
        Template = AiSystemMessages.RequestForAssistantWithArguments,
        InputVariables = [new() { Name = "$term", Description = "The term to translate" }, new (){ Name = "$skip", Description = "The terms to skip due to pagination" }],
        OutputVariable = new OutputVariable
        {
            JsonSchema = """
                         {"term":"adjictable","l":"B1","suggestedTerm": "addictive", "list":[{"tr":"вызывающий привыкание ","l":"B1","e_tr":"Социальные сети могут *вызывать привыкание*","e_or":"Social media can be very *addictive*"}]}
                         """
        }
    });*/
    
    private readonly KernelFunction _translationsFunc = KernelFunctionFactory.CreateFromPrompt(new PromptTemplateConfig
    {
        Template = AiSystemMessages.GiveTranslations,
        InputVariables = [new() { Name = "term", Description = "The term (can be word or phrase to sentence) that must be translated" }],
        OutputVariable = new OutputVariable
        {
            JsonSchema = """
                         {"translations":["to encourage"], "suggested_term": "поощрять", "srcL": "en-US", "outL": "ru-RU"}
                         """
        }
    }); 
    private readonly KernelFunction _alternativesFunc = KernelFunctionFactory.CreateFromPrompt(new PromptTemplateConfig
    {
        Template = AiSystemMessages.giveAlternatives,
        InputVariables = [
            new() { Name = "term", Description = "The term (can be word or phrase to sentence) that must be translated" },
            new() { Name = "existingTranslations", Description = "Existing translations for the term" } ,
            new() { Name = "srclang", Description = "language of the original term" } ,
            new() { Name = "destLang", Description = "language of the translations" } 
        ],
        OutputVariable = new OutputVariable
        {
            JsonSchema = """
                         {"translations":["to encourage"]}
                         """
        }
    });   
    private readonly KernelFunction _giveMotivationalPhraseFunc = KernelFunctionFactory.CreateFromPrompt(new PromptTemplateConfig
    {
        Template = AiSystemMessages.giveMotivationPhase
    });   
    
    private readonly KernelFunction _getLevelFunc = KernelFunctionFactory.CreateFromPrompt(new PromptTemplateConfig
    {
        Template = """
                   There is a $term = '{{$term}}'. Determine the level of complexity from A0 to C2 and return it..
                   Example1: for input term = 'кошка' should return such JSON: {"level": "A1"}
                   Example2: for input term = 'father' should return such JSON: {"level": "A0"}
                   """,
        OutputVariable = new OutputVariable
        {
            JsonSchema = """
                         {"level": "A1"}
                         """
        },
        InputVariables = [new() { Name = "term", Description = "Input word, phrase or sentence" }]
    });
    
    private readonly KernelFunction _examplesFunc = KernelFunctionFactory.CreateFromPrompt(new PromptTemplateConfig
    {
        Template = AiSystemMessages.translationExamples,
        InputVariables = [
            new() { Name = "term", Description = "the original phrase or word that is translated" }, 
            new() { Name = "translations", Description = "Existing translations for the term" } ,
            new() { Name = "srclang", Description = "language of the original term" } ,
            new() { Name = "destLang", Description = "language of the translations" } 
        ],
        OutputVariable = new OutputVariable
        {
            JsonSchema = """
                         {"translations": [{"tr": "translation", "l": "A0", "p": "55", "e_tr": "example of 'translation'", "e_or": "example translated"}]}
                         """
        }
    });
    
    
    private readonly KernelFunction _detectLanguageFunc = KernelFunctionFactory.CreateFromPrompt(new PromptTemplateConfig
    {
        Template = """
                   Detect language and map to the JSON object: {"srcLang": "language of {{$src}} (e.g. en-US)", "destLang": "language of {{$dest}}"}
                   Only these languages are possible: ["en-US", "ru-RU"] 
                   """,
        InputVariables = [
            new() { Name = "src" }, 
            new() { Name = "dest" } ,
        ],
        OutputVariable = new OutputVariable
        {
            JsonSchema = """
                         {"srcLang": "en-US", "destLang": "ru-RU"}
                         """
        }
    });

    public OpenAiGenerator(Kernel kernel, ILogger<OpenAiGenerator> logger)
    {
        this._kernel = kernel;
        _logger = logger;
    }

    /*[Experimental("SKEXP0010")]
    public async Task<TranslationResponse?> GetTranslations(string term)
    {
        KernelArguments arguments = new(new OpenAIPromptExecutionSettings
        {
            ResponseFormat = "json_object"
        }) { { "term", term } };
        
        var result = await _translationsFunc.InvokeAsync<OpenAIChatMessageContent>(_kernel, arguments);

        if (result == null || result.Content == null)
        {
            return null;
        }

        var content = JsonSerializer.Deserialize<TranslationResultNew>(result.Content);

        if (content == null)
        {
            return null;
        }
        
        var items = content!.Translations.Select(t => new TranslationItem(t.Term, t.ExampleTranslated, t.ExampleOriginal, t.Popularity, t.Level));
        var response = new TranslationResponse(content.Term, items, content.Level, content.SuggestedTerm);

        return response;
    }


}*/
    
    [Experimental("SKEXP0010")]
    public async Task<(string srcLang, string destLang)?> DetectLanguage(string src, string dest)
    {
        KernelArguments arguments = new(new OpenAIPromptExecutionSettings
        {
            ResponseFormat = "json_object",
            Temperature = 0.5
        }) { { "src", src }, { "dest", dest }};
        
        var result = await _detectLanguageFunc.InvokeAsync<OpenAIChatMessageContent>(_kernel, arguments);

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
        
        var result = await _examplesFunc.InvokeAsync<OpenAIChatMessageContent>(_kernel, arguments);

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
        
        var result = await _getLevelFunc.InvokeAsync<OpenAIChatMessageContent>(_kernel, arguments);

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
        var result = await _giveMotivationalPhraseFunc.InvokeAsync<OpenAIChatMessageContent>(_kernel);

        return result?.Content;
    }
    
    [Experimental("SKEXP0010")]
    public async Task<SimpleTranslationResult?> GetTranslations(string term)
    {
        _logger.LogInformation("Getting translations for term {term}", term);
        
        KernelArguments arguments = new(new OpenAIPromptExecutionSettings
        {
            ResponseFormat = "json_object",
            Temperature = 0.5
        }) { { "term", term } };
        
        var result = await _translationsFunc.InvokeAsync<OpenAIChatMessageContent>(_kernel, arguments);

        
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

        var response = new SimpleTranslationResult(content.SuggestedTerm, content.Translations, content.SourceLanguage, content.OutputLanguage);

        return response;
    }

    [Experimental("SKEXP0010")]
    public async Task<SimpleTranslationResult?> GetAlternativeTranslations(string term, string sourceLanguage, string destinationLanguage, IEnumerable<string> translations)
    {
        _logger.LogInformation("Getting alternative translations for term {Term}", term);
        
        KernelArguments arguments = new(new OpenAIPromptExecutionSettings
        {
            ResponseFormat = "json_object",
            Temperature = 0.5
        }) { { "term", term }, { "srcLang", sourceLanguage }, { "destLang", destinationLanguage }, { "existingTranslations", JsonSerializer.Serialize(translations) } };
        
        var result = await _alternativesFunc.InvokeAsync<OpenAIChatMessageContent>(_kernel, arguments);
        
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

        var response = new SimpleTranslationResult(content.SuggestedTerm, content.Translations, content.SourceLanguage, content.OutputLanguage);

        return response;
    }
}
    
file class TranslationResult
{
    [JsonPropertyName("term")]
    public string Term { get; set; }
    
    [JsonPropertyName("list")]
    public List<TranslationItemDto> Translations { get; set; }
    
    [JsonPropertyName("suggestedTerm")]
    public string? SuggestedTerm { get; set; }
        
    [JsonPropertyName("l")]
    public string Level { get; set; }
}

file class TranslationItemDto
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
}

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
    public string SourceLanguage { get; set; }
    
    [JsonPropertyName("destLang")]
    public string DestinationLanguage { get; set; }
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
