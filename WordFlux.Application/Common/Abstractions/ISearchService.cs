using WordFlux.Infrastructure.OpenSearch;

namespace WordFlux.Application.Common.Abstractions;



public interface ISearchService
{
    Task<List<TestCard>> SearchAsync(string query, CancellationToken cancellationToken = default);
    Task AddAsync(TestCard card, CancellationToken cancellationToken = default);

    Task CreateIndexAsync(CancellationToken cancellationToken = default);
}