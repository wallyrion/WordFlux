var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .PublishAsAzurePostgresFlexibleServer();

var postgresdb = postgres.AddDatabase("postgresdb");


var apiService = builder
    .AddProject<Projects.WordFlux_ApiService>("apiservice")
    .WithReference(postgresdb);

builder.AddProject<Projects.WordFlux_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService);

builder.Build().Run();
