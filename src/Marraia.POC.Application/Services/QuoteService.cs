using System.Diagnostics;
using System.Net.Http.Json;
using Marraia.POC.Application.Telemetry;
using Microsoft.Extensions.Logging;

namespace Marraia.POC.Application.Services;

public class QuoteService(HttpClient httpClient, ILogger<QuoteService> logger) : IQuoteService
{
    public async Task<decimal> GetDollarAsync(CancellationToken cancellationToken = default)
    {
        using var activity = ApplicationDiagnostics.ActivitySource.StartActivity("QuoteService.GetDollar");
        activity?.SetTag("quote.currency", "USD-BRL");
        activity?.SetTag("quote.provider", httpClient.BaseAddress?.ToString());

        try
        {
            // Calls the external quotes provider configured in ExternalServices:QuotesApi.
            var quote = await httpClient.GetFromJsonAsync<QuoteResponse>("quotes/USD-BRL", cancellationToken)
                ?? throw new InvalidOperationException("Quotes provider returned an empty response.");

            activity?.SetTag("quote.value", quote.Value);
            logger.LogInformation("Dollar quote {Value}", quote.Value);
            return quote.Value;
        }
        catch (HttpRequestException ex)
        {
            activity?.AddException(ex);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            ApplicationDiagnostics.RecordError(ex, "quote.dollar");
            logger.LogError(ex, "Quotes provider {Provider} is unavailable", httpClient.BaseAddress);
            throw;
        }
    }

    private sealed record QuoteResponse(decimal Value);
}
