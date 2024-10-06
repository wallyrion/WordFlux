using WordFLux.ClientApp.Storage;
using WordFlux.Contracts;

namespace WordFLux.ClientApp.Services;

public class LanguageProvider
{
    private List<SupportedLanguage> SupportedLanguages { get; set; } = [];
    private Task<List<SupportedLanguage>>? SupportedLanguagesTask { get; set; }
    private SemaphoreSlim Semaphore { get; } = new SemaphoreSlim(1, 1);
    
    public SupportedLanguage? NativeLanguage { get; set; }
    public SupportedLanguage? LearningLanguage { get; set; }

    private readonly ApiClient _api;
    private readonly LocalStorage _localStorage;
    
    public LanguageProvider(ApiClient api, LocalStorage localStorage)
    {
        _api = api;
        _localStorage = localStorage;
    }

    public async Task<(string NativeLanguageCode, string LearningLanguageCode)> GetDefaultLanguagesAsync()
    {
        return await _localStorage.GetMyLanguages();
    }

    public async Task<List<SupportedLanguage>> GetSupportedLanguagesAsync()
    {
        await Semaphore.WaitAsync();

        try
        {
            SupportedLanguagesTask ??= Task.Run(() => _api.GetSupportedLanguages());
        }
        finally
        {
            Semaphore.Release();
        }

        return await SupportedLanguagesTask;
    }
}