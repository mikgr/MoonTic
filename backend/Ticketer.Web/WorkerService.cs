namespace Ticketer.Web;

public class WorkerService(
    ILogger<WorkerService> logger, 
    IJobQueue jobQueue
    ) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var job = await jobQueue.DequeueAsync(stoppingToken);
            logger.LogInformation("Dequeued background job");
            try
            {
                await job(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error executing background job");
            }
        }
    }
}