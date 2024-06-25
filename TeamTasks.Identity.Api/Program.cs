using System.Threading.RateLimiting;
using TeamTasks.Application;
using TeamTasks.Application.ApiHelpers.Configurations;
using TeamTasks.Application.ApiHelpers.Middlewares;
using TeamTasks.BackgroundTasks;
using TeamTasks.Email;
using TeamTasks.Identity.Api.Common.DependencyInjection;
using TeamTasks.RabbitMq;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.ResponseCompression;
using TeamTasks.Infrastructure;
using TeamTasks.Persistence;
using Prometheus;
using Prometheus.Client.AspNetCore;
using Prometheus.Client.HttpRequestDurations;
using TeamTasks.Identity.Api.Common;
using TeamTasks.Identity.Application;
using ApiVersion = Asp.Versioning.ApiVersion;

#region BuilderRegion

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    
    options.AddPolicy("fixed", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name?.ToString(),
            factory: _ => 
                new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromSeconds(10),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 2
            }));
});

builder.Services
    .AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
        options.Providers.Add<BrotliCompressionProvider>();
        options.Providers.Add<GzipCompressionProvider>();
        options.MimeTypes = ResponseCompressionDefaults.MimeTypes;
    })
    .Configure<BrotliCompressionProviderOptions>(options =>
    {
        options.Level = System.IO.Compression.CompressionLevel.Optimal;
    })
    .Configure<GzipCompressionProviderOptions>(options =>
    {
        options.Level = System.IO.Compression.CompressionLevel.SmallestSize;
    });

builder.Services
    .AddValidators()
    .AddMediatr()
    .AddEndpoints(typeof(DiMediator).Assembly);

builder.Services
    .AddEmailService(builder.Configuration)
    .AddInfrastructure()
    .AddBackgroundTasks(builder.Configuration)
    .AddRabbitMq(builder.Configuration)
    .AddDatabase(builder.Configuration, builder.Environment)
    .AddMetricsOpenTelemetry(builder.Logging)
    .AddSwagger()
    .AddCaching(builder.Configuration)
    .AddLoggingExtension(builder.Configuration);

builder.Services.AddTransient<LogContextEnrichmentMiddleware>();

builder.Services.AddApplication();

builder.Services
    .AddAuthorizationExtension(builder.Configuration)
    .AddCors(options => options.AddDefaultPolicy(corsPolicyBuilder =>
        corsPolicyBuilder.WithOrigins("https://localhost:44442", "http://localhost:44460")
            .AllowAnyHeader()
            .AllowAnyMethod()));

#endregion

//var vaultService = new VaultService(builder.Configuration);
//var service = await vaultService.GetSecretAsync("/v1/secret/data/identity/secret");

#region ApplicationRegion

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerApp();
    app.ApplyMigrations();
}

app.UseRateLimiter();

app.UseCors();

UseMetrics();

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseIdentityServer();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });
});

app.MapControllers();

UseCustomMiddlewares();

MapEndpoints();

app.Run();

#endregion

#region UseMiddlewaresRegion
void UseCustomMiddlewares()
{
    if (app is null)
        throw new ArgumentException();

    app.UseMiddleware<RequestLoggingMiddleware>(app.Logger);
    app.UseMiddleware<ResponseCachingMiddleware>();
    app.UseMiddleware<LogContextEnrichmentMiddleware>();
}

void UseMetrics()
{
    if (app is null)
        throw new ArgumentException();
    
    app.UseMetricServer();
    app.UseHttpMetrics();
    app.UsePrometheusServer();
    app.UsePrometheusRequestDurations();
}

void MapEndpoints()
{
    if (app is null)
        throw new ArgumentException();

    app.MapEndpoints();
}

#endregion