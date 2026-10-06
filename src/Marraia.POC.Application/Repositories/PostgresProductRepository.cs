using Marraia.POC.Domain.Entities;
using Marraia.POC.Domain.Repositories;
using Npgsql;

namespace Marraia.POC.Application.Repositories;

public class PostgresProductRepository(NpgsqlDataSource dataSource) : IProductRepository
{
    public async Task<IReadOnlyCollection<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var command = dataSource.CreateCommand("SELECT id, name, price, created_at FROM products");
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var products = new List<Product>();
        while (await reader.ReadAsync(cancellationToken))
            products.Add(Map(reader));

        return products;
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var command = dataSource.CreateCommand("SELECT id, name, price, created_at FROM products WHERE id = $1");
        command.Parameters.Add(new NpgsqlParameter { Value = id });
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        await using var command = dataSource.CreateCommand(
            "INSERT INTO products (id, name, price, created_at) VALUES ($1, $2, $3, $4)");
        command.Parameters.Add(new NpgsqlParameter { Value = product.Id });
        command.Parameters.Add(new NpgsqlParameter { Value = product.Name });
        command.Parameters.Add(new NpgsqlParameter { Value = product.Price });
        command.Parameters.Add(new NpgsqlParameter { Value = product.CreatedAt });
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static Product Map(NpgsqlDataReader reader)
        => Product.Restore(reader.GetGuid(0), reader.GetString(1), reader.GetDecimal(2), reader.GetDateTime(3));
}
