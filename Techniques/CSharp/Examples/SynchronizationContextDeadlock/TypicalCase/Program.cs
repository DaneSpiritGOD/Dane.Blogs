// See https://aka.ms/new-console-template for more information

using System;
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
        if (!queue.TryDequeue(out var item))
        {
            Thread.Sleep(100);
            continue;
        }

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