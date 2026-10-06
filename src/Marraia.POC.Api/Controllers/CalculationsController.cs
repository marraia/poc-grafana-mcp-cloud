using Marraia.POC.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Marraia.POC.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CalculationsController(ICalculationService calculationService) : ControllerBase
{
    // Divisor defaults to 0 so calling the route without parameters throws DivideByZeroException (HTTP 500).
    [HttpGet("divide")]
    public ActionResult<decimal> Divide([FromQuery] decimal dividend = 10, [FromQuery] decimal divisor = 0)
        => Ok(calculationService.Divide(dividend, divisor));

    // Defaults multiply double.MaxValue by 10, which overflows to Infinity and returns HTTP 500.
    [HttpGet("multiply")]
    public ActionResult<double> Multiply([FromQuery] double multiplicand = double.MaxValue, [FromQuery] double multiplier = 10)
        => Ok(calculationService.Multiply(multiplicand, multiplier));
}
