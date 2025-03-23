using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

using WordFlux.Application.Common.Abstractions;

namespace WordFlux.Translations.Ai;

public class AutocompleteService : IAutocompleteService
{
    private readonly ILogger<AutocompleteService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public AutocompleteService(IServiceProvider serviceProvider, ILogger<AutocompleteService> logger, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
    }

    private Kernel Kernel => _serviceProvider.ResolveKernel(KeyedKernelType.Gpt4oMini);

    
    [Experimental("SKEXP0010")]
    public async Task<(string detectedLanguage, List<(string, string)> autocompletes)?> GetAutocompleteWithTranslations(string term, string lang1, string lang2, CancellationToken cancellationToken = default)
    {
        /*
        var apiKey = _configuration["GeminiAIKey"]!;
        
        IChatClient client = new GeminiChatClient(apiKey, Model.Gemini20Flash);

        var response = await client.GetResponseAsync(AIPrompts.GetAutocompleteWithTranslationsPrompt, new ChatOptions
        {
            ResponseFormat = ChatResponseFormat.Json,
        }, cancellationToken: cancellationToken);
        */
        
        KernelArguments arguments = new(new OpenAIPromptExecutionSettings
        {
            ResponseFormat = "json_object",
            Temperature = 0.5
        }) { { "lang1", lang1 }, { "lang2", lang2 }, { "term", term } };

        
        var result = await AiFunctions.AutocompleteWithTranslationFunc.InvokeAsync<ChatMessageContent>(Kernel, arguments, cancellationToken);
        
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
}

file class AutocompleteWithTranslationsResult
{
    [JsonPropertyName("autocompletes")] public List<AutocompleteTranslationItem> Autocompletes { get; set; }
    [JsonPropertyName("lang")] public string DetectedLanguage { get; set; }
}

file class AutocompleteTranslationItem
{
    [JsonPropertyName("term")] 
    public string AutocompleteResult { get; set; } 
        
    [JsonPropertyName("term_translated")] 
    public string TranslatedAutocompleteResult { get; set; } 
}
