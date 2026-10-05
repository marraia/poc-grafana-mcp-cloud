using Marraia.POC.Application.Repositories;
using Marraia.POC.Application.Services;
using Marraia.POC.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Marraia.POC.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IProductRepository, InMemoryProductRepository>();
        services.AddScoped<IProductService, ProductService>();
        return services;
    }
}
