using Refit;
using TeamTasks.Web.Components;
using TeamTasks.Web.Interfaces.Users;
using TeamTasks.Application.ApiHelpers.Middlewares.DelegatingHandlers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var refitSettings = new RefitSettings
{
    // Настройка обработки ошибок
    ExceptionFactory = async response =>
    {
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return new Exception($"API Error: {content}");
        }
        return null;
    }
};

builder.Services.AddTransient<LoggingHandler>();

//TODO builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(sp.GetRequiredService<IHostEnvironment>().ContentRootPath) });
builder.Services
    .AddRefitClient<IUsersClient>(refitSettings)
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://localhost:7135"))
    .AddHttpMessageHandler<LoggingHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();