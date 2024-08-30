using WordFLux.ClientApp.Services;

namespace WordFLux.ClientApp.Models;

public record TranslationSyncItem(Guid Id, string Term, TranslationSyncStatus Status);