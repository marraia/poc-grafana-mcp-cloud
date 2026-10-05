using System.Collections.Concurrent;
using Marraia.POC.Domain.Entities;
using Marraia.POC.Domain.Repositories;

namespace Marraia.POC.Application.Repositories;

public class InMemoryProductRepository : IProductRepository
{
    private readonly ConcurrentDictionary<Guid, Product> _products = new();

    public Task<IReadOnlyCollection<Product>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyCollection<Product>>(_products.Values.ToList());

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_products.GetValueOrDefault(id));

    public Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        _products[product.Id] = product;
        return Task.CompletedTask;
    }
}
