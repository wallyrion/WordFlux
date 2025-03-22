using WordFlux.Domain.Domain;
using WordFlux.Infrastructure.OpenSearch;

namespace WordFlux.Application.Common.Abstractions;



public interface ISearchService
{
    Task<List<TestCard>> SearchAsync(string query, CancellationToken cancellationToken = default);
    Task<IEnumerable<(Guid cardId, double? Score)>> SearchCardsAsync(string userId, string query, CancellationToken cancellationToken = default);
    Task AddAsync(TestCard card, CancellationToken cancellationToken = default);

    Task CreateIndexAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Card card, CancellationToken cancellationToken = default);
}