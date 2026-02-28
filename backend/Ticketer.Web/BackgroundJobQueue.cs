namespace Ticketer.Web;

using System.Threading.Channels;

public interface IJobQueue
{
    ValueTask EnqueueAsync(Func<CancellationToken, Task> job);
    ValueTask<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);
}


// NB: Temp in-process-queue must be replaced with real infra for PROD
public class JobQueue : IJobQueue
{
    private readonly Channel<Func<CancellationToken, Task>> _queue;

    public JobQueue(int capacity = 500)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        };

        _queue = Channel.CreateBounded<Func<CancellationToken, Task>>(options);
    }

    public async ValueTask EnqueueAsync(Func<CancellationToken, Task> job)
    {
        await _queue.Writer.WriteAsync(job);
    }

    public async ValueTask<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
    {
        return await _queue.Reader.ReadAsync(cancellationToken);
    }
}