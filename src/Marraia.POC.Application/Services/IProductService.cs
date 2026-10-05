using Marraia.POC.Application.Dtos;

namespace Marraia.POC.Application.Services;

public interface IProductService
{
    Task<IReadOnlyCollection<ProductResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProductResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
}
