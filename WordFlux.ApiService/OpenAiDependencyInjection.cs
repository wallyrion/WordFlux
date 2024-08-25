using System.ComponentModel;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel;
using WordFlux.ApiService.Ai;

namespace WordFlux.ApiService;

/*public class TranslationPlugin
{
    [KernelFunction("translate")]
    [Description("Translate text from one language to another.")]
    [return: Description("Translated text.")]
    public string Translate(string text, string from, string to)
    {
        return text;
    }
}*/

public static class OpenAiDependencyInjection
{
    public static IServiceCollection AddOpenAi(this IServiceCollection services, IConfiguration configuration)
    {
        var k = services.AddKernel();
#pragma warning disable SKEXP0001
        k.AddOpenAITextToAudio("tts-1", configuration["OpenAIKey"]);
#pragma warning restore SKEXP0001
        k.AddOpenAIChatCompletion("gpt-4o-mini",
            configuration["OpenAIKey"]!);
        
        services.AddSingleton<OpenAiGenerator>();
        
        return services;

    }
}