using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using TeamTasks.BackgroundTasks.QuartZ.Jobs;

namespace TeamTasks.BackgroundTasks.QuartZ.Schedulers;

/// <summary>
/// Represents the user database scheduler class.
/// </summary>
public sealed class UserDbScheduler
    : AbstractScheduler<BaseDbJob>;