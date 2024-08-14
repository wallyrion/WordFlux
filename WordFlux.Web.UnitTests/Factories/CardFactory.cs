namespace WordFlux.Web.UnitTests.Factories;

public class CardFactory
{
    public static CardDto Create(TimeSpan reviewInternal)
    {
        return new CardDto(Guid.NewGuid(), DateTime.UtcNow, "term", "A0",
            [new TranslationItem("Translation1", "example of translation 1", "example of original 1", 30, "A1")], reviewInternal);
    }
}