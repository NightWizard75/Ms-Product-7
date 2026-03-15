using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Application.Shared.Pipeline;
using MediatR;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        
        services.AddValidatorsFromAssembly(assembly);
        
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
