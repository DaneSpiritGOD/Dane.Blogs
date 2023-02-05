# Let's talk about deadlock related to SynchronizationContext

This kind of issue has been discussed much on the Internet. See [Don't Block on Async Code](https://blog.stephencleary.com/2012/07/dont-block-on-async-code.html)

Most of people say `Task.Result` or `Task.GetAwaiter().GetResult()` might cause deadlock at UI-context runtime. UI-context thread is blocked and completion callback inside Task being awaited is waiting for UI context... **But what on earth does such deadlock model look like in code base?**

## What's UI context?

Before I show you my understanding, I wish you to know what is this kind of context and what it is used for first.

Said by Microsoft, [SynchronizationContext](https://learn.microsoft.com/en-us/dotnet/api/system.threading.synchronizationcontext?view=net-7.0)

> Provides the basic functionality for propagating a synchronization context in various synchronization models.

There is another post written by Stephen Cleary, which introduced history of this class and a serial of demonstrations, comparisons, analysis of implementations in fine details. [It's All About the SynchronizationContext](https://learn.microsoft.com/en-us/archive/msdn-magazine/2011/february/msdn-magazine-parallel-computing-it-s-all-about-the-synchronizationcontext). It's a long read but please keep patient and enjoy it. Nothing else looks better than it.

## Create a customized synchronization model

To establish a model, which is used to track the deadlock, we need to figure out how synchronization context is used. The first idea is to print some trace logs during the methods of `SynchronizationContext` are called.

A new `SynchronizationContext` class type is the first of this model.

``` cs
class MySynchronizationContext : SynchronizationContext
{
    readonly ConcurrentQueue<(Action<object?>, object?)> queue;
    readonly TextWriter tw;

    public MySynchronizationContext(ConcurrentQueue<(Action<object?>, object?)> queue, TextWriter tw)
    {
        this.queue = queue;
        this.tw = tw;
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
```
