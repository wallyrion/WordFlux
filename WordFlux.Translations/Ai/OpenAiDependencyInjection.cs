using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using WordFlux.Application;
using WordFlux.Application.Common.Abstractions;
using WordFlux.Translations.AzureAiTranslator;

namespace WordFlux.Translations.Ai;

#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0010

public static class OpenAiDependencyInjection
{
    [Experimental("SKEXP0070")]
    public static IServiceCollection AddSemanticKernels(this IServiceCollection services, IConfiguration configuration)
    {
        var apiKey = configuration["OpenAIKey"]!;
        var geminiKey = configuration["GeminiAIKey"]!;

        services.AddTextAudioKernel(geminiKey);
        services.AddOpenAiChatCompletionKernels(apiKey, KeyedKernelType.Gpt4o, KeyedKernelType.Gpt4oMini);
        services.AddGeminiKeyAiChatCompletionKernels(geminiKey, KeyedKernelType.GeminiFlash, KeyedKernelType.Gemini15Flash);

        services.AddSingleton<OpenAiGenerator>();
        services.AddSingleton<AzureAiTranslationService>();
        services.AddSingleton<IOpenAiGenerator, OpenAiGenerator>(s => s.GetRequiredService<OpenAiGenerator>());
        services.AddSingleton<IAzureAiTranslator, AzureAiTranslationService>(s => s.GetRequiredService<AzureAiTranslationService>());
        services.AddKeyedSingleton<ITranslationService, AzureAiTranslationService>("AzureAiTranslator");
        services.AddKeyedSingleton<ITranslationService, OpenAiTranslationService>("OpenAiTranslator");

        services.AddSingleton<IAutocompleteService, AutocompleteService>();
        

        services.AddSingleton<IAudioAiGenerator, AudioAiGenerator>();
        //services.AddSingleton<ITranslationService, AzureAiTranslationService>();
        //services.AddSingleton<ITranslationService, OpenAiTranslationService>();

        return services;
    }

    private static IServiceCollection AddOpenAiChatCompletionKernels(this IServiceCollection services, string apiKey, params KeyedKernelType[] types)
    {
        foreach (var model in types)
        {
            var modelName = GetModelNameByType(model);
            var kernelBuilder = Kernel.CreateBuilder();
            kernelBuilder.AddOpenAIChatCompletion(modelName, apiKey);
            var kernel = kernelBuilder.Build();
            services.AddKeyedSingleton(model, kernel);
        }

        return services;
    } 
    
    [Experimental("SKEXP0070")]
    private static IServiceCollection AddGeminiKeyAiChatCompletionKernels(this IServiceCollection services, string apiKey, params KeyedKernelType[] types)
    {
        foreach (var model in types)
        {
            var modelName = GetModelNameByType(model);
            var kernelBuilder = Kernel.CreateBuilder();
            kernelBuilder.AddGoogleAIGeminiChatCompletion(modelName, apiKey);
            var kernel = kernelBuilder.Build();
            services.AddKeyedSingleton(model, kernel);
        }

        return services;
    }

    private static IServiceCollection AddTextAudioKernel(this IServiceCollection services, string apiKey)
    {
        var name = GetModelNameByType(KeyedKernelType.AudioText);
        var audioTextKernelBuilder = Kernel.CreateBuilder();
        audioTextKernelBuilder.AddOpenAITextToAudio(name, apiKey);
        var audioTextKernel = audioTextKernelBuilder.Build();

        services.AddKeyedSingleton(KeyedKernelType.AudioText, audioTextKernel);

        return services;
    }

    private static string GetModelNameByType(KeyedKernelType type) => type switch
    {
        KeyedKernelType.AudioText => "tts-1",
        KeyedKernelType.Gpt4oMini => "gpt-4o-mini",
        KeyedKernelType.Gpt4o => "gpt-4o",
        KeyedKernelType.GeminiFlash => "gemini-2.0-flash",
        KeyedKernelType.Gemini15Flash => "gemini-1.5-flash",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };

    public static ITranslationService ResolveTranslationService(this IServiceProvider di, bool useAzureAitranslator)
    {
        return di.GetRequiredKeyedService<ITranslationService>(useAzureAitranslator ? "AzureAiTranslator" : "OpenAiTranslator");
    }
    
    public static Kernel ResolveKernel(this IServiceProvider di, KeyedKernelType model)
    {
        return di.GetRequiredKeyedService<Kernel>(model);
    }
}