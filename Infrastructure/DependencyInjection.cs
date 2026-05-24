using Application.Shared.Interfaces;
using Infrastructure.BackgroundServices;
using Infrastructure.Database.Context;
using Infrastructure.Options;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddDbContext<ProductDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ProductDbContext).Assembly.FullName)));

        services.AddScoped<IProductRepository, ProductRepository>();
        
        // 👇 RabbitMQ settings
        services.Configure<RabbitMQSettings>(configuration.GetSection("RabbitMQ"));

        // 👇 RabbitMQ publisher (для исходящих событий)
        services.AddScoped<IRabbitMqPublisher, RabbitMqPublisher>();

        // 👇 RabbitMQ hosted service (для входящих событий)
        services.AddHostedService<RabbitMqHostedService>();

        return services;
    }
}
