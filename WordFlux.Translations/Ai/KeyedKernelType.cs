namespace WordFlux.Translations.Ai;

public enum KeyedKernelType
{
    AudioText,
    Gpt4oMini,
    Gpt4o,
    GeminiFlash,
    Gemini15Flash,
}

public enum AiProvider
{
    OpenAI,
    Gemini
}

public static class AiProviderExtensions
{
    public static AiProvider GetProvider(this KeyedKernelType kernelType)
    {
        return kernelType switch
        {
            KeyedKernelType.Gemini15Flash or KeyedKernelType.GeminiFlash => AiProvider.Gemini,
            _ => AiProvider.OpenAI
        };
    }    
}
