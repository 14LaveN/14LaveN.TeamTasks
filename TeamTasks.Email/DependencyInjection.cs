using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TeamTasks.Email.Emails;
using TeamTasks.Email.Emails.Settings;

namespace TeamTasks.Email;

public static class DependencyInjection
{
    public static IServiceCollection AddEmailService(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        if (services is null)
            throw new ArgumentException();
    
        services.Configure<MailSettings>(configuration.GetSection(MailSettings.SettingsKey));
        
        services.AddOptions<MailSettings>()
            .BindConfiguration(MailSettings.SettingsKey)
            .ValidateOnStart();
        
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IEmailNotificationService, EmailNotificationService>();
        
        return services;
    }
}