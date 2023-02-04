namespace DispatcherModel;

using System.Collections.Concurrent;

public class MyApplication
{
    public static void Run(MyWindow window)
    {
        var tw = Console.Out;

        var previousSynchronizationContext = SynchronizationContext.Current;

        ConcurrentQueue<(Action<object?>, object?)> queue = new();
        var mySynchronizationContext = new MySynchronizationContext(queue, tw);
        SynchronizationContext.SetSynchronizationContext(mySynchronizationContext);
        
        try
        {
            mySynchronizationContext.Post(s =>
            {
                window.Show();
            }, null);

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
    }
}