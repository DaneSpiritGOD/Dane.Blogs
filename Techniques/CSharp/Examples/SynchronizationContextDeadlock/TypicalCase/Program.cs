// See https://aka.ms/new-console-template for more information

using DispatcherModel;
using Workload;

var window = new MyWindow();
window.Showed += delegate {
    var myJob = new MyJob();
    myJob.DoWork(10);
};

MyApplication.Run(window);