using MassTransit.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace WordFlux.Infrastructure.Observability;

public static class OpenTelemetryDependencyInjection
{
    public static ILoggingBuilder AddLogging(this ILoggingBuilder logging, IConfiguration configuration)
    {
        var seqServer = configuration["Seq:Server"];
        var key = configuration["Seq:ApiKey"];
        
        logging
            .AddSeq(seqServer, key)
            .AddOpenTelemetry(l =>
            {
                l.IncludeFormattedMessage = true;
                l.IncludeScopes = true;
                l.AddOtlpExporter();
            });

        return logging;
    }
    
    private static Action<OtlpExporterOptions> ConfigureSeqExporter(IConfiguration configuration)
    {
        var seqServer = configuration["Seq:Server"];
        var key = configuration["Seq:ApiKey"];

        return c =>
        {
            c.Endpoint = new Uri($"{seqServer}/ingest/otlp/v1/traces");
            c.Headers = $"X-Seq-ApiKey={key}";
            c.Protocol = OtlpExportProtocol.HttpProtobuf;
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

            metrics.AddOtlpExporter();
            //metrics.AddOtlpExporter(ConfigureAspireDashboardExporter(configuration));
        });

        otel.WithTracing(tracing =>
        {
            tracing
                .AddSource("Sample.DistributedTracing")
                .AddSource(DiagnosticHeaders.DefaultListenerName) // MassTransit ActivitySource
                .AddSource("Microsoft.SemanticKernel*")
                ;

            tracing.AddAspNetCoreInstrumentation();
            tracing.AddHttpClientInstrumentation(h =>
            {
                h.FilterHttpRequestMessage = FilterOutSeqRequests(configuration);
            });
            
            tracing.AddEntityFrameworkCoreInstrumentation();
            tracing.AddOtlpExporter();
            tracing.AddOtlpExporter(ConfigureSeqExporter(configuration));
        });

        return services;
    }

    private static Func<HttpRequestMessage, bool> FilterOutSeqRequests(IConfiguration configuration)
    {
        var seqServer = configuration["Seq:Server"];
        return message =>
        {
            Console.WriteLine($"Sending trace request to {message.RequestUri?.AbsoluteUri}");
            var isSeqUrl = message.RequestUri?.AbsoluteUri.Contains(seqServer!);
            return isSeqUrl == null || !isSeqUrl.Value;
        };
    }
}
