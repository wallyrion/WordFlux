using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WordFlux.Domain;
using WordFlux.Infrastructure.Persistence;

namespace WordFlux.Infrastructure.Authorization;

public static class DependencyInjection
{
    public static IServiceCollection AddWordfluxAuthorization(this IServiceCollection services)
    {
        services.AddIdentityApiEndpoints<AppUser>()
            .AddRoles<IdentityRole>()
            .AddRoleManager<RoleManager<IdentityRole>>()
            .AddEntityFrameworkStores<ApplicationDbContext>();
        
        services.AddAuthorization();
        services.AddOptions<BearerTokenOptions>(IdentityConstants.BearerScheme)
            .Configure(options => { options.BearerTokenExpiration = TimeSpan.FromDays(7); });
        
        return services;
    }

    public static async Task AddKeysProtectionPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var blobStorageConnectionString = configuration.GetConnectionString("KeysPersistenceBlobStorage");

        if (string.IsNullOrEmpty(blobStorageConnectionString))
        {
            return;
        }
        
        string containerName = "protection-keys";
        string blobName = "keys.xml";
        BlobContainerClient container = new BlobContainerClient(blobStorageConnectionString, containerName);

        await container.CreateIfNotExistsAsync();

        BlobClient blobClient = container.GetBlobClient(blobName);
        
        services.AddDataProtection()
            .PersistKeysToAzureBlobStorage(blobClient);
    }
}