using Marraia.POC.Application.Repositories;
using Marraia.POC.Application.Services;
using Marraia.POC.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Marraia.POC.Application;

public static class DependencyInjection
{
    public const string DatabaseRepositoryKey = "database";

    public static IServiceCollection AddApplication(this IServiceCollection services, string? productsConnectionString, string? quotesApiUrl)
    {
        services.AddSingleton<IProductRepository, InMemoryProductRepository>();
        services.AddSingleton(_ => NpgsqlDataSource.Create(productsConnectionString
            ?? throw new InvalidOperationException("Connection string 'ProductsDatabase' is not configured.")));
        services.AddKeyedSingleton<IProductRepository, PostgresProductRepository>(DatabaseRepositoryKey);
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICalculationService, CalculationService>();
        services.AddHttpClient<IQuoteService, QuoteService>(client =>
            client.BaseAddress = new Uri(quotesApiUrl
                ?? throw new InvalidOperationException("Setting 'ExternalServices:QuotesApi' is not configured.")));
        return services;
    }
}
