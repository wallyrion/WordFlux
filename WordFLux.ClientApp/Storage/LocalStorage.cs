using Blazored.LocalStorage;

namespace WordFLux.ClientApp.Storage;

public class LocalStorage(ILocalStorageService localStorage)
{
    const string LatestDecksIds = "latestDecks";
    public async Task<Guid> GetMyId()
    {
        if (await localStorage.ContainKeyAsync("myId"))
        {
            return await localStorage.GetItemAsync<Guid>("myId");
        }

        var id = Guid.NewGuid();
        await localStorage.SetItemAsync("myId", id);

        return id;
    }
    
    public async Task SaveNativeLanguage(string langCode)
    {
        await localStorage.SetItemAsync("languages_native", langCode);
    }

    public async Task SaveStudyingLanguage(string langCode)
    {
        await localStorage.SetItemAsync("languages_studying", langCode);
    }

    public async Task<(string native, string studing)> GetMyLanguages()
    {
        var native = await localStorage.GetItemAsync<string>("languages_native");
        var studying = await localStorage.GetItemAsync<string>("languages_studying");

        return (native ?? "ru", studying ?? "en");
    }

    public async Task<List<Guid>> GetLatestDecksSelection()
    {
        return (await localStorage.GetItemAsync<List<Guid>>(LatestDecksIds)) ?? [] ;
    }

    public async Task SaveDeckSelection(List<Guid> decks)
    {
        await localStorage.SetItemAsync(LatestDecksIds, decks);
    }
}