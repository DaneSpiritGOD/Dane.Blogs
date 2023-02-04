// See https://aka.ms/new-console-template for more information

using System.Threading;
using DispatcherModel;
using Workload;

Console.Out.WriteLine("Adopting solution 2");

var window = new MyWindow();
window.Showed += delegate {
    var currentSynchronizationContext = SynchronizationContext.Current;
    SynchronizationContext.SetSynchronizationContext(null);

    try
    {
        var myjob = new MyJob();
        myjob.DoWork(10);
    }
    finally
    {
        SynchronizationContext.SetSynchronizationContext(currentSynchronizationContext);
    }
};

MyApplication.Run(window);