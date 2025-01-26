using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenSearch.Client;
using OpenSearch.Net;
using WordFlux.Application;
using WordFlux.Application.Common.Abstractions;
using WordFlux.Application.Common.Options;
using WordFlux.Infrastructure.Authorization;
using WordFlux.Infrastructure.OpenSearch;
using WordFlux.Infrastructure.Persistence;

namespace WordFlux.Infrastructure;

public static class DependecyInjectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHostedService<MigrationHostedService>();
        services.AddScoped<ICurrentUser, CurrentUser>();

        var openSearchSettings = configuration.GetSection("Opensearch").Get<OpensearchOptions>();
        var settings = new ConnectionSettings(new Uri(openSearchSettings!.Url))
            .BasicAuthentication(openSearchSettings.Username, openSearchSettings.Password)
            .DefaultIndex(SearchService.DefaultIndex);

        if (openSearchSettings.SkipSslVerification)
        {
            settings = settings.ServerCertificateValidationCallback(CertificateValidations.AllowAll);
        }

        var client = new OpenSearchClient(settings);
        services.AddSingleton(client);
        services.AddSingleton<ISearchService, SearchService>();
        
        return services;
    }
}