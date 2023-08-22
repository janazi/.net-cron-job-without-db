using Cronos;

namespace CronJobApi.CronJobs;

public abstract class CronJobService<T> : IHostedService, IDisposable
{
    private System.Timers.Timer? _timer;
    private readonly CronExpression _expression;
    private readonly TimeZoneInfo _timeZoneInfo;

    protected CronJobService(IScheduleConfig<T> scheduleConfig)
    {
        ArgumentNullException.ThrowIfNull(scheduleConfig);
        _expression = CronExpression.Parse(scheduleConfig.CronExpression);
        _timeZoneInfo = scheduleConfig.TimeZoneInfo;
    }

    public virtual async Task StartAsync(CancellationToken cancellationToken)
    {
        await ScheduleJob(cancellationToken);
    }

    protected virtual async Task ScheduleJob(CancellationToken cancellationToken)
    {
        var next = _expression.GetNextOccurrence(DateTimeOffset.Now, _timeZoneInfo);
        if (!next.HasValue) return;

        var delay = next.Value - DateTimeOffset.Now;
        if (delay.TotalMilliseconds <= 0)   // prevent non-positive values from being passed into Timer
        {
            await ScheduleJob(cancellationToken);
        }
        _timer = new System.Timers.Timer(delay.TotalMilliseconds);
        _timer.Elapsed += async (_, _) =>
        {
            _timer.Dispose();  // reset and dispose timer
            _timer = null;

            if (!cancellationToken.IsCancellationRequested)
            {
                await DoWork(cancellationToken);
            }

            if (!cancellationToken.IsCancellationRequested)
            {
                await ScheduleJob(cancellationToken);    // reschedule next
            }
        };
        _timer.Start();

        await Task.CompletedTask;
    }

    public virtual async Task DoWork(CancellationToken cancellationToken)
    {
        await Task.Delay(5000, cancellationToken);  // do the work
    }

    public virtual async Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Stop();
        await Task.CompletedTask;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
            _timer?.Dispose();
    }
}

// ReSharper disable once UnusedTypeParameter
public interface IScheduleConfig<T>
{
    string CronExpression { get; set; }
    TimeZoneInfo TimeZoneInfo { get; set; }
}

public class ScheduleConfig<T> : IScheduleConfig<T>
{
    public string CronExpression { get; set; } = string.Empty;
    public TimeZoneInfo TimeZoneInfo { get; set; } = TimeZoneInfo.Local;
}

public static class ScheduledServiceExtensions
{
    public static IServiceCollection AddCronJob<T>(this IServiceCollection services, IConfiguration configuration)
        where T : CronJobService<T>
    {
        var config = new ScheduleConfig<T>();
        configuration.GetSection($"CronJobs:{typeof(T).Name}").Bind(config);
        services.AddSingleton<IScheduleConfig<T>>(config);
        services.AddHostedService<T>();
        return services;
    }
}