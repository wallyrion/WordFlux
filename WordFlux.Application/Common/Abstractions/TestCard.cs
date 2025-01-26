using WordFlux.Infrastructure.OpenSearch;

namespace WordFlux.Application.Common.Abstractions;

public class TestCard
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Term { get; set; }
    public string SrcLang { get; set; }
    public List<TestCardTranslation> Translations { get; set; }
}