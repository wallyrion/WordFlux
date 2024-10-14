using MassTransit.Logging;
using Microsoft.AspNetCore.HttpLogging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;

namespace WordFlux.ApiService.Infrastructure;

public static class OpenTelemetryDependencyInjection
{

    public static ILoggingBuilder AddLogging(this ILoggingBuilder logging, IConfiguration configuration, IServiceCollection builderServices)
    {
        builderServices.AddHttpLogging(l =>
        {
            l.CombineLogs = true;
            l.LoggingFields = HttpLoggingFields.All;

        });

        
        logging
            .AddSerilog(new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger())
            .AddOpenTelemetry(l =>
            {
                l.IncludeFormattedMessage = true;
                l.IncludeScopes = true;
                l.AddOtlpExporter(ConfigureAspireDashboardExporter(configuration));
            });

        return logging;
    }
    
    private static Action<OtlpExporterOptions> ConfigureAspireDashboardExporter(IConfiguration configuration)
    {
        var aspireDashboardEndpoint = new Uri(configuration["OtelEndpointAspireDashboard"]!);
        var aspireDashboardHeaders = configuration["OtelHeadersAspireDashboard"]!;
        
        return c =>
        {
            c.Endpoint = aspireDashboardEndpoint;
            c.Headers = aspireDashboardHeaders;
        };
    }
    
    public static IServiceCollection AddTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        var otel = services.AddOpenTelemetry();


        
        // Add Metrics for ASP.NET Core and our custom metrics and export via OTLP
        otel.WithMetrics(metrics =>
        {
            // Metrics provider from OpenTelemetry
            metrics.AddAspNetCoreInstrumentation();
            //Our custom metrics
            metrics.AddMeter("Microsoft.SemanticKernel*");
            // Metrics provides by ASP.NET Core in .NET 8
            metrics.AddMeter("Microsoft.AspNetCore.Hosting");
            metrics.AddMeter("Microsoft.AspNetCore.Server.Kestrel");
            
            metrics.AddOtlpExporter(ConfigureAspireDashboardExporter(configuration));
        });

        otel.WithTracing(tracing =>
        {
            tracing
                .AddSource("Sample.DistributedTracing")
                .AddSource(DiagnosticHeaders.DefaultListenerName) // MassTransit ActivitySource
                .AddSource("Microsoft.SemanticKernel*")
                ;

            tracing.AddAspNetCoreInstrumentation();
            tracing.AddHttpClientInstrumentation();
            tracing.AddEntityFrameworkCoreInstrumentation();


            tracing.AddOtlpExporter(ConfigureAspireDashboardExporter(configuration));

            tracing.AddOtlpExporter("seq", c =>
            {
                c.Endpoint = new Uri(configuration["OtelEndpointSeq"]!);
                c.Headers = configuration["OtelHeadersSeq"];
                c.Protocol = OtlpExportProtocol.HttpProtobuf;
            });
        });

        return services;
    }
}