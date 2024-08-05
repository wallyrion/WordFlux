var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.WordFlux_ApiService>("apiservice");

builder.AddProject<Projects.WordFlux_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService);

builder.Build().Run();
