using Blazored.LocalStorage;

namespace WordFlux.Web.Storage;

public class LocalStorage
{
    private readonly ILocalStorageService _localStorage;

    public LocalStorage(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task<Guid> GetMyId()
    {
        if (await _localStorage.ContainKeyAsync("myId"))
        {
            return await _localStorage.GetItemAsync<Guid>("myId");
        }

        var id = Guid.NewGuid();
        await _localStorage.SetItemAsync("myId", id);

        return id;
    }
    
    public async Task SaveNativeLanguage(string langCode)
    {
        await _localStorage.SetItemAsync("languages_native", langCode);
    }

    public async Task SaveStudyingLanguage(string langCode)
    {
        await _localStorage.SetItemAsync("languages_studying", langCode);
    }

    public async Task<(string native, string studing)> GetMyLanguages()
    {
        var native = await _localStorage.GetItemAsync<string>("languages_native");
        var studying = await _localStorage.GetItemAsync<string>("languages_studying");

        return (native ?? "ru-RU", studying ?? "en-US");
    }
}