namespace CronJobApi.CronJobs;

public class Worker1 : CronJobService<Worker1>
{
    private readonly ILogger<Worker1> _logger;
    public Worker1(IScheduleConfig<Worker1> cronJobSettings,
        ILogger<Worker1> logger)
        : base(cronJobSettings)
    {
        _logger = logger;
    }

    public override Task DoWork(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{now} Worker 1 is working.", DateTime.Now.ToString("T"));
        return Task.CompletedTask;
    }
}