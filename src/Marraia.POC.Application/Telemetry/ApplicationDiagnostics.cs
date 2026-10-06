using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Marraia.POC.Application.Telemetry;

public static class ApplicationDiagnostics
{
    public const string SourceName = "Marraia.POC.Application";

    public static readonly ActivitySource ActivitySource = new(SourceName);

    private static readonly Meter Meter = new(SourceName);

    public static readonly Counter<long> ProductsCreated =
        Meter.CreateCounter<long>("poc.products.created", unit: "{product}", description: "Number of products created");

    public static readonly Histogram<double> ProductPrice =
        Meter.CreateHistogram<double>("poc.products.price", unit: "BRL", description: "Price of created products");

    public static readonly Counter<long> Errors =
        Meter.CreateCounter<long>("poc.errors", unit: "{error}", description: "Number of errors raised by the application");

    public static void RecordError(Exception exception, string operation)
        => Errors.Add(1,
            new KeyValuePair<string, object?>("error.type", exception.GetType().FullName),
            new KeyValuePair<string, object?>("operation", operation));
}
