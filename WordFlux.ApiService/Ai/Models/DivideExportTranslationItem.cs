namespace WordFlux.ApiService.Ai.Models;

public class QuizletMapExportItem
{
    public required string Term { get; set; }
    public string? SourceLanguage { get; set; }
    public string? DestinationLanguage { get; set; }

    public string? Definition { get; set; }
    public List<QuizletMapExportItemTranslation> Translations { get; set; } = [];
}

public class QuizletMapExportItemTranslation
{
    public required string Translation { get; set; }
    public string? Example { get; set; }
}