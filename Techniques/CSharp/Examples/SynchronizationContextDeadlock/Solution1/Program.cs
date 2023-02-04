// See https://aka.ms/new-console-template for more information

using DispatcherModel;
using Workload;

Console.Out.WriteLine("Adopting solution 1");

var window = new MyWindow();
window.Showed += delegate {
    var myjob = new MyJobWrapper();
    myjob.DoWork(10);
};

MyApplication.Run(window);

class MyJobWrapper : MyJob
{
    protected override async Task<int> DoWorkAsync(int seed)
    {
        // await Task.Delay(0).ConfigureAwait(false); // not work

        await Task.Delay(1).ConfigureAwait(false); // work, but it will release execution priority
        return await base.DoWorkAsync(seed);
    }
}