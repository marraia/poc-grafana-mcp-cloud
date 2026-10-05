using Marraia.POC.Application.Dtos;
using Marraia.POC.Application.Telemetry;
using Marraia.POC.Domain.Entities;
using Marraia.POC.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Marraia.POC.Application.Services;

public class ProductService(IProductRepository repository, ILogger<ProductService> logger) : IProductService
{
    public async Task<IReadOnlyCollection<ProductResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var activity = ApplicationDiagnostics.ActivitySource.StartActivity("ProductService.GetAll");

        var products = await repository.GetAllAsync(cancellationToken);
        activity?.SetTag("products.count", products.Count);
        logger.LogInformation("Listed {Count} products", products.Count);

        return products.Select(ToResponse).ToList();
    }

    public async Task<ProductResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var activity = ApplicationDiagnostics.ActivitySource.StartActivity("ProductService.GetById");
        activity?.SetTag("product.id", id);

        var product = await repository.GetByIdAsync(id, cancellationToken);
        if (product is null)
        {
            logger.LogWarning("Product {ProductId} not found", id);
            return null;
        }

        return ToResponse(product);
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        using var activity = ApplicationDiagnostics.ActivitySource.StartActivity("ProductService.Create");

        var product = new Product(request.Name, request.Price);
        await repository.AddAsync(product, cancellationToken);

        activity?.SetTag("product.id", product.Id);
        ApplicationDiagnostics.ProductsCreated.Add(1);
        ApplicationDiagnostics.ProductPrice.Record((double)product.Price);
        logger.LogInformation("Product {ProductId} created with name {ProductName} and price {ProductPrice}",
            product.Id, product.Name, product.Price);

        return ToResponse(product);
    }

    private static ProductResponse ToResponse(Product product)
        => new(product.Id, product.Name, product.Price, product.CreatedAt);
}
