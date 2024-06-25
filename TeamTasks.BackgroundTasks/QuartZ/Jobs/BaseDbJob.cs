using Microsoft.Extensions.Logging;
using Quartz;
using TeamTasks.Persistence;
using static System.Console;

namespace TeamTasks.BackgroundTasks.QuartZ.Jobs;

/// <summary>
/// Represents the base database job.
/// </summary>
public sealed class BaseDbJob : IJob
{
    private readonly BaseDbContext _appDbContext = new();

    /// <inheritdoc />
    public async System.Threading.Tasks.Task Execute(IJobExecutionContext context)
    {
        await _appDbContext.SaveChangesAsync();
        WriteLine($"All.SaveChanges - {DateTime.UtcNow}");
    }
}