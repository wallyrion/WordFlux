using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace WordFlux.Application.Common.Abstractions;

public interface IAutocompleteService
{
    public Task<(string detectedLanguage, List<(string, string)> autocompletes)?> GetAutocompleteWithTranslations(string term, string lang1, string lang2, CancellationToken cancellationToken = default);
}