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
}
