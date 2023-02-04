// See https://aka.ms/new-console-template for more information

using System.Collections.Concurrent;
using TypicalCase;

var tw = Console.Out;

var previousSynchronizationContext = SynchronizationContext.Current;
try
{
    ConcurrentQueue<(Action<object?>, object?)> queue = new();
    var mySynchronizationContext = new MySynchronizationContext(queue, tw);
    SynchronizationContext.SetSynchronizationContext(mySynchronizationContext);

    mySynchronizationContext.Post(s =>
    {
        tw.WriteLine("******************** Before GetResult call ********************");
        var result = DoWorkAsync((int)s!).GetAwaiter().GetResult();
        tw.WriteLine($"******************** After GetResult call. Result = {result} ********************");
    }, 13);

    while (true)
    {
        tw.WriteLine("^^^^^^^^^^^^^^^^^^^^ Looking for a job... ^^^^^^^^^^^^^^^^^^^^");
        if (!queue.TryDequeue(out var item))
        {
            tw.WriteLine("^^^^^^^^^^^^^^^^^^^^ No job found ^^^^^^^^^^^^^^^^^^^^");
            Thread.Sleep(100);
            continue;
        }

        tw.WriteLine("^^^^^^^^^^^^^^^^^^^^ A job found. Executing... ^^^^^^^^^^^^^^^^^^^^");
        item.Item1(item.Item2);
    }
}
finally
{
    SynchronizationContext.SetSynchronizationContext(previousSynchronizationContext);
}

async Task<int> DoWorkAsync(int seed)
{
    seed *= 12;

    var resultOnTask = await Task.Run(() =>
    {
        var sum = 0;
        for (var i = 0; i < 10000; ++i)
        {
            sum += i;
        }
        return sum;
    });
    return seed + resultOnTask / 10;
}