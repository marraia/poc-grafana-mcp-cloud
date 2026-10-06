using System.Diagnostics;
using Marraia.POC.Application.Telemetry;
using Microsoft.Extensions.Logging;

namespace Marraia.POC.Application.Services;

public class CalculationService(ILogger<CalculationService> logger) : ICalculationService
{
    public decimal Divide(decimal dividend, decimal divisor)
    {
        using var activity = ApplicationDiagnostics.ActivitySource.StartActivity("CalculationService.Divide");
        activity?.SetTag("calculation.dividend", dividend);
        activity?.SetTag("calculation.divisor", divisor);

        try
        {
            // decimal division throws DivideByZeroException when divisor is 0.
            var result = dividend / divisor;
            activity?.SetTag("calculation.result", result);
            logger.LogInformation("Division {Dividend} / {Divisor} = {Result}", dividend, divisor, result);
            return result;
        }
        catch (DivideByZeroException ex)
        {
            activity?.AddException(ex);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            ApplicationDiagnostics.RecordError(ex, "calculation.divide");
            logger.LogError(ex, "Failed to divide {Dividend} by {Divisor}", dividend, divisor);
            throw;
        }
    }
}
