namespace Marraia.POC.Application.Dtos;

public record CreateProductRequest(string Name, decimal Price);

public record ProductResponse(Guid Id, string Name, decimal Price, DateTime CreatedAt);
