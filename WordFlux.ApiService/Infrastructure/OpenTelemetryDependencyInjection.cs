using System.Diagnostics;
using MassTransit.Logging;
using Microsoft.AspNetCore.HttpLogging;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Instrumentation.Http;
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

        var seqServer = configuration["Seq:Server"];
        var key = configuration["Seq:ApiKey"];
        
        logging
            //.AddSerilog(new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger())
            .AddSeq(seqServer, key)
            .AddOpenTelemetry(l =>
            {
                l.IncludeFormattedMessage = true;
                l.IncludeScopes = true;
                l.AddOtlpExporter();
                //l.AddOtlpExporter(ConfigureAspireDashboardExporter(configuration));
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
            //tracing.AddOtlpExporter(ConfigureAspireDashboardExporter(configuration));
            tracing.AddOtlpExporter(ConfigureSeqExporter(configuration));
        });

        //otel.UseOtlpExporter();
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
