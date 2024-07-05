using TeamTasks.BackgroundTasks;
using TeamTasks.Database.MetricsAndRabbitMessages;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddHealthChecksUI().AddInMemoryStorage();

builder.Services.AddMongoDatabase(builder.Configuration);

builder.Services.AddBackgroundTasks(builder.Configuration);

var app = builder.Build();

app.UseRouting(); 

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    
    endpoints.MapHealthChecksUI();
});

app.Run();