using Microsoft.Extensions.Logging;
using OpenSearch.Client;
using WordFlux.Application.Common.Abstractions;
using WordFlux.Domain.Domain;

namespace WordFlux.Infrastructure.OpenSearch;

public class SearchService(OpenSearchClient client, ILogger<SearchService> logger) : ISearchService
{
    public const string DefaultIndex = "test-cards2";

    private Dictionary<string, string> SupportedLanguageAnalyzers = new()
    {
        { "en", "english" },
        { "ru", "russian" },
        { "ua", "russian" },
        { "es", "spanish" },
        { "default", "simple" }
    };

    public async Task CreateIndexAsync(CancellationToken cancellationToken = default)
    {
        var creationResponse = await client.Indices.CreateAsync(DefaultIndex, c => c
            .Map(m => m
                .Properties(p =>
                {
                    p = p.Keyword(k => k.Name("CardId"))
                        .Keyword(k => k.Name("UserId"));
                    
                    // Iterate through SupportedLanguageAnalyzers to create mappings dynamically
                    foreach (var language in SupportedLanguageAnalyzers)
                    {
                        p = p.Text(t => t
                            .Name($"Term_{language.Key}") 
                            .Analyzer(language.Value)); 

                        p = p.Text(t => t
                            .Name($"Translation_{language.Key}") 
                            .Analyzer(language.Value)); 
                    }
                    return p;
                })
            ), cancellationToken);

        if (!creationResponse.IsValid)
        {
            logger.LogError(creationResponse.OriginalException, "Failed to create default index");
            throw new Exception("Failed to create default index", creationResponse.OriginalException);
        }
    }

    public async Task<List<TestCard>> SearchAsync2(string query, CancellationToken cancellationToken = default)
    {
        var cards = new List<TestCard>
        {
            // 1. Multiple Spanish translations
            new TestCard
            {
                Term = "Happy",
                SrcLang = "en",
                Translations = new List<TestCardTranslation>
                {
                    new TestCardTranslation { Language = "es", Text = "Feliz" },
                    new TestCardTranslation { Language = "es", Text = "Contento" }, // Alternative
                    new TestCardTranslation { Language = "fr", Text = "Heureux" }
                }
            },

            // 2. Multiple German translations
            new TestCard
            {
                Term = "Fast",
                SrcLang = "en",
                Translations = new List<TestCardTranslation>
                {
                    new TestCardTranslation { Language = "de", Text = "Schnell" },
                    new TestCardTranslation { Language = "de", Text = "Rasant" }, // Alternative
                    new TestCardTranslation { Language = "it", Text = "Veloce" }
                }
            },

            // 3. Multiple Japanese translations
            new TestCard
            {
                Term = "Thank you",
                SrcLang = "en",
                Translations = new List<TestCardTranslation>
                {
                    new TestCardTranslation { Language = "ja", Text = "ありがとう" }, // Arigatou
                    new TestCardTranslation { Language = "ja", Text = "感謝します" }, // Kansha shimasu (more formal)
                    new TestCardTranslation { Language = "ko", Text = "감사합니다" }
                }
            },

            // 4. Multiple French translations
            new TestCard
            {
                Term = "Small",
                SrcLang = "en",
                Translations = new List<TestCardTranslation>
                {
                    new TestCardTranslation { Language = "fr", Text = "Petit" },
                    new TestCardTranslation { Language = "fr", Text = "Menu" }, // Alternative
                    new TestCardTranslation { Language = "es", Text = "Pequeño" }
                }
            },

            // 5. Multiple Russian translations
            new TestCard
            {
                Term = "Beautiful",
                SrcLang = "en",
                Translations = new List<TestCardTranslation>
                {
                    new TestCardTranslation { Language = "ru", Text = "Красивый" },
                    new TestCardTranslation { Language = "ru", Text = "Прекрасный" }, // Alternative
                    new TestCardTranslation { Language = "ar", Text = "جميل" }
                }
            },
            new TestCard
            {
                Term = "running",
                SrcLang = "en",
                Translations = new List<TestCardTranslation>
                {
                    new TestCardTranslation { Language = "ru", Text = "бегущий" }, // to run (imperfective)
                    new TestCardTranslation { Language = "ru", Text = "бегать" }, // to run (habitual)
                    new TestCardTranslation { Language = "es", Text = "correr" }
                }
            },
            /*new Card
            {
                Term = "бегать",
                SrcLang = "ru",
                Translations = new List<CardTranslation>
                {
                    new CardTranslation { Language = "en", Text = "to run" }, // to run (imperfective)
                }
            },
            new Card
            {
                Term = "бігати",
                SrcLang = "ua",
                Translations = new List<CardTranslation>
                {
                    new CardTranslation { Language = "en", Text = "run" }, // to run (imperfective)
                }
            },*/
            new TestCard
            {
                Term = "Books",
                SrcLang = "en",
                Translations = new List<TestCardTranslation>
                {
                    new TestCardTranslation { Language = "ru", Text = "книги" }, // books (plural)
                    new TestCardTranslation { Language = "ru", Text = "книга" }, // book (singular)
                    new TestCardTranslation { Language = "de", Text = "Bücher" }
                }
            }
        };

        foreach (var card in cards)
        {
            var doc = new Dictionary<string, object>
            {
                { $"Term_{card.SrcLang}", card.Term },
                { "UserId", card.Term },
            };

            foreach (var translation in card.Translations.GroupBy(x => x.Language))
            {
                doc.Add($"Translation_{translation.Key}", translation.Select(c => c.Text).ToArray());
            }

            await client.IndexAsync(doc, i => i
                    .Index(DefaultIndex)
                    .Id(card.Id)
                ,
                cancellationToken);
        }

        var searchTerm = "run"; // Replace with your search term
        var searchResponse = client.Search<Dictionary<string, object>>(s => s
            .Explain(true)
            .Index(DefaultIndex)
            .Query(q => q
                .MultiMatch(mm => mm
                    .Fields(f => f
                            // .Field("Translation_*") // Search across all language-specific Translation fields
                            // .Field("Term_*") // Search across all language-specific Term fields
                            .Field("english_term")
                            .Field("Term_*") // Search across all language-specific Term fields
                    )
                    .Query(searchTerm)
                )
            )
        );

        var searchResponse2 = client.Search<Dictionary<string, object>>(s => s
                .Index(DefaultIndex) // Specify the index name
                .Query(q => q
                        .MatchAll() // Match all documents
                )
                .Size(1000) // Adjust the size to retrieve more documents if needed
        );

        return [];
    }
    
    
    public async Task<List<TestCard>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        var searchResponse = client.Search<Dictionary<string, object>>(s => s
            .Index(DefaultIndex)
            .Query(q => q
                .MultiMatch(mm => mm
                    .Fields(f => f
                            .Field("Translation_*")
                            .Field("Term_*")

                    )
                    .Query(query)
                )
            )
        );

        var getAll = client.Search<Dictionary<string, object>>(s => s
                .Index(DefaultIndex) // Specify the index name
                .Query(q => q
                        .MatchAll() // Match all documents
                )
                .Size(1000) // Adjust the size to retrieve more documents if needed
        );

        var response = searchResponse.Hits.Select(hit =>
        {
            var term = hit.Source.First(c => c.Key.StartsWith("Term")).Value.ToString();
            var card = new TestCard
            {
                Id = Guid.Parse(hit.Id),
                Term = term!,
                Translations = hit.Source.Where(c => c.Key.StartsWith("Translation")).SelectMany(c =>
                {
                    var lang = c.Key[..c.Key.IndexOf('_')];
                    var translations = c.Value as List<object>;
                    var r1 = translations!.Cast<string>().Select(t => new TestCardTranslation
                    {
                        Language = lang,
                        Text = t
                    });

                    return r1;
                }).ToList()
            };

            return card;
        });

        return response.ToList();
    }
    public async Task<IEnumerable<(Guid cardId, double? Score)>> SearchCardsAsync(string userId, string query, CancellationToken cancellationToken = default)
    {
        var searchResponse = client.Search<Dictionary<string, object>>(s => s
            .Index(DefaultIndex)
            .Query(q => q
                .Bool(b => b
                    .Must(mu => mu
                            .MultiMatch(mm => mm
                                .Fields(f => f
                                    .Field("Translation_*")
                                    .Field("Term_*")
                                )
                                .Query(query)
                            ),
                        mu => mu
                            .Term(t => t
                                .Field("UserId")
                                .Value(userId)
                            )
                    )
                )
            )
        );

        var getAll = client.Search<Dictionary<string, object>>(s => s
                .Index(DefaultIndex) // Specify the index name
                .Query(q => q
                        .MatchAll() // Match all documents
                )
                .Size(1000) // Adjust the size to retrieve more documents if needed
        );

        var searchResults = searchResponse.Hits.Select(hit =>
        {
            var cardId = Guid.Parse(hit.Id);
            return (cardId, hit.Score);
        });

        return searchResults;
    }

    public async Task AddAsync(TestCard card, CancellationToken cancellationToken = default)
    {
        var doc = new Dictionary<string, object>
        {
            { $"Term_{card.SrcLang}", card.Term },
            { "UserId", card.Term },
        };

        foreach (var translation in card.Translations.GroupBy(x => x.Language))
        {
            doc.Add($"Translation_{translation.Key}", translation.Select(c => c.Text).ToArray());
        }

        await client.IndexAsync(doc, i => i
                .Index(DefaultIndex)
                .Id(card.Id)
            ,
            cancellationToken);

        await client.Indices.RefreshAsync(DefaultIndex);
    }
    
    public async Task AddAsync(Card card, CancellationToken cancellationToken = default)
    {
        var srcLang = card.SourceLanguage ?? "default";
        var targetLang = card.TargetLanguage ?? "default";
        
        var translations = card.Translations.Select(t => t.Term).ToArray();
        var doc = new Dictionary<string, object>
        {
            { $"Term_{srcLang}", card.Term },
            { "UserId", card.CreatedBy },
            { $"Translation_{targetLang}", translations }
        };
        
        /*foreach (var translation in card.Translations.GroupBy(x => x.Language))
        {
            doc.Add($"Translation_{translation.Key}", translation.Select(c => c.Text).ToArray());
        }*/

        await client.IndexAsync(doc, i => i
                .Index(DefaultIndex)
                .Id(card.Id)
            ,
            cancellationToken);

        await client.Indices.RefreshAsync(DefaultIndex, ct: cancellationToken);
    }
}


record SearchCardItem(Guid CardId, int Score);