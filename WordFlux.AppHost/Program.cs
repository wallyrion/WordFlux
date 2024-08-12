using Aspire.Hosting.Azure;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Before creating builder");
var builder = DistributedApplication.CreateBuilder(args);
Console.WriteLine("After creating builder");

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .PublishAsAzurePostgresFlexibleServer();

var postgresdb = postgres.AddDatabase("postgresdb");

IResourceBuilder<AzureKeyVaultResource>? secrets = null;

if (builder.ExecutionContext.IsPublishMode)
{
    secrets = builder.AddAzureKeyVault("secrets");
}



var apiService = builder
        .AddProject<Projects.WordFlux_ApiService>("apiservice")
        .WithReference(postgresdb)
        .WaitFor(postgresdb)
    ;

if (secrets != null)
{
    apiService = apiService.WithReference(secrets);
}

builder.AddProject<Projects.WordFlux_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService);

builder.Build().Run();