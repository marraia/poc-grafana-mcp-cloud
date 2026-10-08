using Marraia.POC.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Marraia.POC.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuotesController(IQuoteService quoteService) : ControllerBase
{
    // Depends on the external quotes provider; when it is down the HttpClient call fails and the API returns HTTP 500.
    [HttpGet("dollar")]
    public async Task<ActionResult<decimal>> GetDollar(CancellationToken cancellationToken)
        => Ok(await quoteService.GetDollarAsync(cancellationToken));
}
