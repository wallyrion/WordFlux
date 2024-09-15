using System.ComponentModel;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel;
using WordFlux.ApiService.Ai;
using WordFlux.ApiService.Services;

namespace WordFlux.ApiService;

#pragma warning disable SKEXP0001

public static class OpenAiDependencyInjection
{
    public static IServiceCollection AddOpenAi(this IServiceCollection services, IConfiguration configuration)
    {
        var k = services.AddKernel();
        k.AddOpenAITextToAudio("tts-1", configuration["OpenAIKey"]!);
        k.AddOpenAIChatCompletion("gpt-4o-mini",
            configuration["OpenAIKey"]!);
        
        services.AddSingleton<OpenAiGenerator>();
        services.AddKeyedSingleton<ITranslationService, AzureAiTranslationService>("AzureAiTranslator");
        services.AddKeyedSingleton<ITranslationService, OpenAiTranslationService>("OpenAiTranslator");
        //services.AddSingleton<ITranslationService, AzureAiTranslationService>();
        //services.AddSingleton<ITranslationService, OpenAiTranslationService>();
        
        return services;

    }

    public static ITranslationService ResolveTranslationService(this IServiceProvider di, bool useAzureAitranslator)
    {
        return di.GetRequiredKeyedService<ITranslationService>(useAzureAitranslator ? "AzureAiTranslator" : "OpenAiTranslator");
    }
}