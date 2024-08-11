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
}