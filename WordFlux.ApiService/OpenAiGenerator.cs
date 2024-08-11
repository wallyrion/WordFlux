using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace WordFlux.ApiService;

public class OpenAiGenerator
{
    private readonly Kernel _kernel;
    private readonly KernelFunction _translationsFunc = KernelFunctionFactory.CreateFromPrompt(new PromptTemplateConfig
    {
        Template = AiSystemMessages.RequestForAssistantWithArguments,
        InputVariables = [new() { Name = "$term", Description = "The term to translate" }, new (){ Name = "$skip", Description = "The terms to skip due to pagination" }],
        OutputVariable = new OutputVariable
        {
            JsonSchema = """
                         {"term":"adjictable", "suggestedTerm": "addictive", "list":[{"tr":"вызывающий привыкание ","e_tr":"Социальные сети могут *вызывать привыкание*","e_or":"Social media can be very *addictive*"}]}
                         """
        }
    });

    public OpenAiGenerator(Kernel kernel)
    {
        this._kernel = kernel;
    }

    [Experimental("SKEXP0010")]
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

        var content = JsonSerializer.Deserialize<TranslationResult>(result.Content);

        if (content == null)
        {
            return null;
        }
        
        var items = content!.Translations.Select(t => new TranslationItem(t.Term, t.ExampleTranslated, t.ExampleOriginal, t.Popularity));
        var response = new TranslationResponse(content.Term, items, content.SuggestedTerm);

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
}
