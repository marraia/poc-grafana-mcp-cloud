using Marraia.POC.Application.Dtos;
using Marraia.POC.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Marraia.POC.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(IProductService productService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ProductResponse>>> GetAll(CancellationToken cancellationToken)
        => Ok(await productService.GetAllAsync(cancellationToken));

    // Reads from PostgreSQL; with no database available the connection fails and the API returns HTTP 500.
    [HttpGet("database")]
    public async Task<ActionResult<IReadOnlyCollection<ProductResponse>>> GetAllFromDatabase(CancellationToken cancellationToken)
        => Ok(await productService.GetAllFromDatabaseAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var product = await productService.GetByIdAsync(id, cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<ProductResponse>> Create(CreateProductRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var product = await productService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
