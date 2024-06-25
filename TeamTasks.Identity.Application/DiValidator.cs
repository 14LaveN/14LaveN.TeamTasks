using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TeamTasks.Domain.Core.Utility;
using TeamTasks.Identity.Application.Commands;

namespace TeamTasks.Identity.Application;

public static class DiValidator
{
    /// <summary>
    /// Registers the necessary services with the DI framework.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddValidators(this IServiceCollection services)
    {
        Ensure.NotNull(services, "Services is required.", nameof(services));

        services
            .AddScoped<IValidator<ChangeName.Command>, ChangeName.CommandValidator>()
            .AddScoped<IValidator<ChangePassword.Command>, ChangePassword.CommandValidator>()
            .AddScoped<IValidator<Login.Command>, Login.CommandValidator>()
            .AddScoped<IValidator<Register.Command>, Register.CommandValidator>();
        
        return services;
    }
}