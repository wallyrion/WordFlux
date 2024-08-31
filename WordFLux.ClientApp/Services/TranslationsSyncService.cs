using Blazored.LocalStorage;
using WordFLux.ClientApp.Models;

namespace WordFLux.ClientApp.Services;

public class TranslationsSyncService(
    ILogger<TranslationsSyncService> logger,
    ILocalStorageService localStorageService,
    InMemoryMessageQueue queue,
    IServiceProvider serviceCollection,
    ConnectionHealthService healthService)
{
    private const string Key = "OutOfSyncTranslations";
    public event Action? OnStateChanged;

    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task<List<TranslationSyncItem>> GetAll()
    {
        var result = await localStorageService.GetItemAsync<List<TranslationSyncItem>>(Key);

        return result ?? [];
    }

    private async Task UpdateItem(TranslationSyncItem item)
    {
        using var _ = await _semaphore.CreateLockScopeAsync();

        var items = await localStorageService.GetItemAsync<List<TranslationSyncItem>>(Key);

        if (items == null || items.Count == 0)
        {
            return;
        }

        var existingIndex = items.FindIndex(x => x.Id == item.Id);
        items[existingIndex] = item;
        await localStorageService.SetItemAsync(Key, items);
    }

    private async Task RemoveItem(Guid id)
    {
        using var _ = await _semaphore.CreateLockScopeAsync();

        var items = await localStorageService.GetItemAsync<List<TranslationSyncItem>>(Key);

        if (items == null || items.Count == 0)
        {
            return;
        }

        var filtered = items.Where(x => x.Id != id).ToList();
        await localStorageService.SetItemAsync(Key, filtered);
    }

    public async Task<TranslationSyncItem> Add(string term)
    {
        using var lockScope = await _semaphore.CreateLockScopeAsync();

        var newItem = new TranslationSyncItem(Guid.NewGuid(), term, TranslationSyncStatus.NotSynced);
        var items = await GetAll();
        items.Add(newItem);
        await localStorageService.SetItemAsync(Key, items);

        if (healthService.IsOnline)
        {
            await queue.Writer.WriteAsync(new ApplicationEvent(Type.ConnectionOnline));

            return newItem;
        }

        return newItem;
    }

    public async Task WaitForEventAndSyncTranslations()
    {
        logger.LogInformation("starting WaitForEventAndSyncTranslations sync");

        await foreach (var integrationEvent in
                       queue.Reader.ReadAllAsync())
        {
            logger.LogInformation("Get event {Event}", integrationEvent.Type);

            if (integrationEvent.Type != Type.ConnectionOnline)
            {
                continue;
            }

            if (!healthService.IsOnline)
            {
                continue;
            }

            logger.LogInformation("Processing event");

            var items = await GetAll();
            logger.LogInformation("Get items, Items = {@Items}", items);

            foreach (var item in items)
            {
                if (!healthService.IsOnline)
                {
                    continue;
                }

                var updatedItem = item with { Status = TranslationSyncStatus.InProgress };
                await UpdateItem(updatedItem);
                OnStateChanged?.Invoke();

                await using var scope = serviceCollection.CreateAsyncScope();
                logger.LogInformation("CreatedScope for term {Term}", item);

                var apiClient = scope.ServiceProvider.GetRequiredService<WeatherApiClient>();
                logger.LogInformation("Took api client");

                await SaveTranslationSilent(item.Term, apiClient);

                await RemoveItem(item.Id);
                OnStateChanged?.Invoke();
            }
        }
    }

    private async Task SaveTranslationSilent(string term, WeatherApiClient apiClient)
    {
        try
        {
            logger.LogInformation("Processing item Term = {Term}", term);

            var translations = await apiClient.GetSimpleTranslations(term);

            var examples = await apiClient.GetTranslationExamples(term, translations.Translations, translations.SourceLanguage,
                translations.DestinationLanguage);

            var normalizedTerm = string.IsNullOrWhiteSpace(translations.SuggestedTerm) ? term : translations.SuggestedTerm;
            var level = await apiClient.GetLevel(normalizedTerm);

            await apiClient.SaveNewCard(new CardRequest(normalizedTerm, level, examples));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while syncing term: {Term}", term);
        }

    }
}

public enum TranslationSyncStatus
{
    NotSynced,
    InProgress,
    Synced,
    Error
}