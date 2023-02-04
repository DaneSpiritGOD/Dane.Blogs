// See https://aka.ms/new-console-template for more information

using DispatcherModel;
using Workload;

var window = new MyWindow();
window.Showed += delegate {
    var workload = new MyJob();
    workload.DoWork(10);
};

MyApplication.Run(window);