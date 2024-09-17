using WordFlux.Contracts;

namespace WordFLux.ClientApp.Models;

/*public record CardTranslationItem(string Term, string ExampleTranslated, string ExampleOriginal, int Popularity, string Level);*/


public record GetTranslationExamplesRequest(string Term, List<string> Translations, string SourceLanguage, string DestinationLanguage);
