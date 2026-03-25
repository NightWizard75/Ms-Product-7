using System.Reflection;
using FluentValidation;

namespace Web;

public static class DependencyInjection
{
    public static IServiceCollection AddWeb(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        
        // 👇 Валидаторы для Web-DTO (CreateProductRequestValidator и др.)
        services.AddValidatorsFromAssembly(assembly);
    
        // 👇 Контроллеры + Swagger
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c => 
        {
            c.SwaggerDoc("v1", new() { Title = "Product Service API", Version = "v1" });
        });

        return services;
    }
}
