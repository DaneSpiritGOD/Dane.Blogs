using System.Collections.Concurrent;

namespace DispatcherModel;

class MySynchronizationContext : SynchronizationContext
{
    readonly ConcurrentQueue<(Action<object?>, object?)> queue;
    readonly TextWriter tw;

    public MySynchronizationContext(ConcurrentQueue<(Action<object?>, object?)> queue, TextWriter tw)
    {
        this.queue = queue;
        this.tw = tw;
    }

    public override SynchronizationContext CreateCopy()
    {
        tw.WriteLine($"{nameof(CreateCopy)} being called");
        return new MySynchronizationContext(queue, tw);
    }

    public override void OperationStarted()
    {
        tw.WriteLine($"{nameof(OperationStarted)} being called");
    }

    public override void OperationCompleted()
    {
        tw.WriteLine($"{nameof(OperationCompleted)} being called");
    }

    public override void Post(SendOrPostCallback d, object? state)
    {
        tw.WriteLine($@"-------------------- {nameof(Post)} being called --------------------
{Environment.StackTrace}");
        queue.Enqueue((s => d(s), state));
        tw.WriteLine($"-------------------- {nameof(Post)} call exiting --------------------");
    }

    public override void Send(SendOrPostCallback d, object? state)
    {
        tw.WriteLine($@"-------------------- {nameof(Send)} being called --------------------
{Environment.StackTrace}");
        d(state);
        tw.WriteLine($"-------------------- {nameof(Send)} call exiting --------------------");
    }
}