/*using Aspire.Hosting.Azure;
using Microsoft.Extensions.Hosting;
using WordFlux.AppHost.Extensions;*/

Console.WriteLine("Before creating builder");
var builder = DistributedApplication.CreateBuilder(args);
Console.WriteLine("After creating builder");

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .PublishAsAzurePostgresFlexibleServer();

var postgresdb = postgres.AddDatabase("postgresdb");

var apiService = builder
        .AddProject<Projects.WordFlux_ApiService>("apiservice")
        .WithReference(postgresdb)
        .WaitFor(postgresdb)
    ;


/*
builder.AddProject<Projects.WordFlux_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService);*/

builder.Build().Run();