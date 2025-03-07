﻿using Microsoft.AspNetCore.HttpLogging;

namespace WordFlux.ApiService.ServiceCollectionExtensions;

public static class HttpLoggingExtensions
{
    public static IServiceCollection AddWordfluxHttpLogging(this IServiceCollection builderServices)
    {
        builderServices.AddHttpLogging(l =>
        {
            l.CombineLogs = true;
            l.LoggingFields = HttpLoggingFields.All;
        });

        return builderServices;
    }
}
