using Marraia.POC.Application.Telemetry;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Marraia.POC.Api.Extensions;

public static class OpenTelemetryExtensions
{
    public static WebApplicationBuilder AddObservability(this WebApplicationBuilder builder)
    {
        var settings = builder.Configuration.GetSection("OpenTelemetry");
        var serviceName = settings["ServiceName"] ?? builder.Environment.ApplicationName;
        var endpoint = (settings["Endpoint"] ?? "http://localhost:4318").TrimEnd('/');
        var protocol = settings["Protocol"] == "grpc" ? OtlpExportProtocol.Grpc : OtlpExportProtocol.HttpProtobuf;
        var headers = settings["Headers"];
        var resourceAttributes = ParseKeyValues(settings["ResourceAttributes"]);
        resourceAttributes["deployment.environment"] = builder.Environment.EnvironmentName;

        // With http/protobuf each signal needs its own path (/v1/traces, /v1/metrics, /v1/logs).
        void ConfigureExporter(OtlpExporterOptions options, string signalPath)
        {
            options.Protocol = protocol;
            options.Endpoint = protocol == OtlpExportProtocol.HttpProtobuf
                ? new Uri($"{endpoint}/{signalPath}")
                : new Uri(endpoint);
            if (!string.IsNullOrWhiteSpace(headers))
                options.Headers = headers;
        }

        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName, serviceVersion: typeof(OpenTelemetryExtensions).Assembly.GetName().Version?.ToString())
                .AddAttributes(resourceAttributes))
            .WithTracing(tracing => tracing
                .AddSource(ApplicationDiagnostics.SourceName)
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter(options => ConfigureExporter(options, "v1/traces")))
            .WithMetrics(metrics => metrics
                .AddMeter(ApplicationDiagnostics.SourceName)
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddOtlpExporter(options => ConfigureExporter(options, "v1/metrics")))
            .WithLogging(logging => logging
                .AddOtlpExporter(options => ConfigureExporter(options, "v1/logs")));

        return builder;
    }

    private static Dictionary<string, object> ParseKeyValues(string? value)
    {
        var result = new Dictionary<string, object>();
        if (string.IsNullOrWhiteSpace(value))
            return result;

        foreach (var pair in value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var parts = pair.Split('=', 2, StringSplitOptions.TrimEntries);
            if (parts.Length == 2)
                result[parts[0]] = parts[1];
        }

        return result;
    }
}
