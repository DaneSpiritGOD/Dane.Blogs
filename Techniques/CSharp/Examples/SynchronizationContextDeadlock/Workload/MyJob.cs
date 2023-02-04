namespace Workload;

public class MyJob
{
    public int DoWork(int seed)
    {
        Console.Out.WriteLine("******************** Before GetResult call ********************");

        var result = default(int);
        try
        {
            return result = DoWorkAsync(seed).GetAwaiter().GetResult();
        }
        finally
        {
            Console.Out.WriteLine($"******************** After GetResult call. Result = {result} ********************");
        }
    }

    protected virtual async Task<int> DoWorkAsync(int seed)
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
}
