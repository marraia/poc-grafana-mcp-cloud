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

    public double Multiply(double multiplicand, double multiplier)
    {
        using var activity = ApplicationDiagnostics.ActivitySource.StartActivity("CalculationService.Multiply");
        activity?.SetTag("calculation.multiplicand", multiplicand);
        activity?.SetTag("calculation.multiplier", multiplier);

        try
        {
            // double multiplication does not throw on overflow: it silently yields Infinity.
            var result = multiplicand * multiplier;
            if (double.IsInfinity(result))
                throw new OverflowException($"Multiplication {multiplicand} * {multiplier} resulted in an infinite number.");

            activity?.SetTag("calculation.result", result);
            logger.LogInformation("Multiplication {Multiplicand} * {Multiplier} = {Result}", multiplicand, multiplier, result);
            return result;
        }
        catch (OverflowException ex)
        {
            activity?.AddException(ex);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            ApplicationDiagnostics.RecordError(ex, "calculation.multiply");
            logger.LogError(ex, "Failed to multiply {Multiplicand} by {Multiplier}", multiplicand, multiplier);
            throw;
        }
    }
}
