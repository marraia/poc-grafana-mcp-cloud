using Marraia.POC.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Marraia.POC.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CalculationsController(ICalculationService calculationService) : ControllerBase
{
    // Divisor defaults to 1 and a zero divisor is rejected with HTTP 400 instead of surfacing a 500.
    [HttpGet("divide")]
    public ActionResult<decimal> Divide([FromQuery] decimal dividend = 10, [FromQuery] decimal divisor = 1)
    {
        if (divisor == 0)
            return BadRequest(new { error = "Divisor must not be zero." });

        return Ok(calculationService.Divide(dividend, divisor));
    }
}
