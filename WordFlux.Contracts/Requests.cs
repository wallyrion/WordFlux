﻿namespace WordFlux.Contracts;

public record CardTranslationItem(string Term, string? ExampleTranslated = null, string? ExampleOriginal = null, int Popularity = 0, string? Level = null)
{
    public bool IsSelected { get; set; }
}

public class CardTaskExample
{
    public required string ExampleLearn { get; set; }
    public required string ExampleNative { get; set; }
}

/*public record CardTranslationViewModel(string Term, string? ExampleTranslated = null, string? ExampleOriginal = null, int Popularity = 0, string? Level = null)
    : CardTranslationItem(Term, ExampleTranslated, ExampleOriginal, Popularity, Level)
{
    public bool IsSelected { get; set; }
}*/


public class PatchCardRequest
{
    public string? Term { get; init; }
    public Guid? DeckId { get; init; }
    public string? ImageUrl { get; init; }
    public List<CardTranslationItem>? Translations { get; init; }
}

public record CardRequest(
    string Term,
    string Level,
    List<CardTranslationItem> Translations,
    Guid DeckId = default,
    string? ImageUrl = null,
    string? NativeLang = null,
    string? LearnLang = null,
    string? SourceLang = null,
    string? TargetLang = null);

public record GetTranslationExamplesRequest(string Term, List<string> Translations, string SourceLanguage, string DestinationLanguage);

public record GetAutocompleteRequest(string Term, string SourceLanguage, string DestinationLanguage);

public record GetAutocompleteResponse(string DetectedLanguage, List<string> Completions);

public record CreateDeckRequest(string Name);

public record PatchDeckRequest(string? Name = null, bool? IsPublic = null);

public record ImportDeckRequest(string? Name, string Cards, string NativeLanguage, string LearnLanguage);
public record ParseExportQuizletResponse();

public record ParseExportQuizletItem();



public class DeckExportPayload
{
    public string NativeLanguage { get; set; } = null!;
    public string LearnLanguage { get; set; } = null!;
    public DeckExportStatus Status { get; set; }
    
    public List<DeckExportItem> Items { get; set; } = [];
}

public enum DeckExportStatus
{
    Processing,
    Completed
}

public class DeckExportItem
{
    public string Term { get; set; }
    public string? Translation { get; set; }
}