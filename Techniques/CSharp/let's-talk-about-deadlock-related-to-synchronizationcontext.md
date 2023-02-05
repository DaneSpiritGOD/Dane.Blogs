# Let's talk about deadlock related to SynchronizationContext

This kind of issue has been discussed much on the Internet. See [Don't Block on Async Code](https://blog.stephencleary.com/2012/07/dont-block-on-async-code.html)

Most of people say `Task.Result` or `Task.GetAwaiter().GetResult()` might cause deadlock at UI-context runtime. UI-context thread is blocked and completion callback inside Task being awaited is waiting for UI context... **But what on earth does such deadlock model look like in code base?**

## What's UI context?

Before I show you my understanding, I wish you to know what is this kind of context and what it is used for first.

Said by Microsoft, [SynchronizationContext](https://learn.microsoft.com/en-us/dotnet/api/system.threading.synchronizationcontext?view=net-7.0)

> Provides the basic functionality for propagating a synchronization context in various synchronization models.

There is another post written by Stephen Cleary, which introduced history of this class and a serial of demonstrations, comparisons, analysis of implementations in fine details. [It's All About the SynchronizationContext](https://learn.microsoft.com/en-us/archive/msdn-magazine/2011/february/msdn-magazine-parallel-computing-it-s-all-about-the-synchronizationcontext). It's a long read but please keep patient and enjoy it. Nothing else looks better than it.

## Create a customized synchronization model

In my imagination, this model should look like the one in wpf/winform, a single-thread world. Every change to UI is supposed to be done on main thead. There is a message pump on main thread to receive messages of operations in an infinite loop. Actually, wpf has its own `System.Windows.Threading.Dispatcher` or winform has its own `System.ComponentModel.ISynchronizeInvoke`. It looks like they have nothing to do with `SynchronizationContext`. As we need something to propagate synchronization context to base layer, which has no awareness of wpf or winform. Then it is a kind of abstraction for generic purpose to be introduced.

Having the fundamental background, we can create a message handling loop by our own to realize a single-thread context model. One thread in this model continuously queries message from a queue. If one message is got, we run the action stored in the message. If no message is found, we do nothing but wait for a message to come up in next iteration.

``` cs
// message loop
while (true)
{
    if (!queue.TryDequeue(out var item))
    {
        Thread.Sleep(100);
        continue;
    }

    item.Item1(item.Item2);
}
```

Next question is who puts message into the queue. Obviously, it must be another thread. But how could it know the existence of that *message queue* as the queue is very specific to *"main thead"*? We can think of `SynchronizationContext` now. `SynchronizationContext` has a static member called `Current`, which can be obtained by different threads. The value via call to `Current` depends on what kind of context is installed on **current** thread.

`SynchronizationContext` has several overridable members. For concrete-issue reason, we are just concerned about `Post` and `Send` method.

[SynchronizationContext.Post](https://learn.microsoft.com/en-us/dotnet/api/system.threading.synchronizationcontext.post?view=net-7.0),

> When overridden in a derived class, dispatches an asynchronous message to a synchronization context.

In `Post`, we queue the `callback` and `state` into *message queue* that is used by *message loop* thread. Then the message we put here will be obtained by *main thread* and the action would be executed by it.

[SynchronizationContext.Send](https://learn.microsoft.com/en-us/dotnet/api/system.threading.synchronizationcontext.send?view=net-7.0),

> When overridden in a derived class, dispatches a synchronous message to a synchronization context.

In implementation of `Send`, we call the `callback` directly on current thread, since I found this method has nothing to do with our topic finally.

``` cs
class MySynchronizationContext : SynchronizationContext
{
    public MySynchronizationContext(ConcurrentQueue<(Action<object?>, object?)> queue, TextWriter tw)
    {
        this.queue = queue;
        this.tw = tw;
    }

    public override void Post(SendOrPostCallback d, object? state)
    {
        queue.Enqueue((s => d(s), state));
    }

    public override void Send(SendOrPostCallback d, object? state)
    {
        d(state);
    }
}
```

OK, now we have a simple dispatcher model and a target-specific sync context, how can we relate them? We referred to that `SynchronizationContext` can be installed on thread above. We can then install the context before starting the *message loop* so that our *main thread* has our own synchronization context.

``` cs
var previousSynchronizationContext = SynchronizationContext.Current; // capture the previous

var mySynchronizationContext = new MySynchronizationContext(queue, tw);
SynchronizationContext.SetSynchronizationContext(mySynchronizationContext); // install ours

try
{
    // do meesage loop here
    // ...
}
finally
{
    SynchronizationContext.SetSynchronizationContext(previousSynchronizationContext); // restore context
}
```

With respects to how our synchronization context is propagated to any other thread automatically, it's a thing belonging to dotnet framework, not ours.

### How does user code get run usually?

In wpf or winform, everything starts with *main window* and all stuffs happening on the *main window* should be managed by that *message pump*. As well, I referred to the implementation of wpf, `mainWindow.Show` is wrapped as a *message*, which is posted to *pump*. Then our dispatcher looks like,

``` cs
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

            // do message loop here
            // ...
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(previousSynchronizationContext);
        }
    }
}

public class MyWindow
{
    public void Show()
    {
        OnShow();
    }

    public event EventHandler? Showed;
    protected virtual void OnShow()
    {
        Showed?.Invoke(this, EventArgs.Empty);
    }
}
```

## Come back to our real topic - *deadlock*

It's a long preparation but rather a brief introduction to common UI synchronization model. A normal way to build an UI application is to designing a main window and handling events such as *Click*, *SizeChanged*, *Showed*, etc.

Typical *deadlock* occurs when we wait for an asynchronization method to complete by call to `Task.Result` or `Task.GetAwaiter().GetResult()` in such event handlers.

I create a job which is to simulate normal case of heavy workload.

``` cs
public class MyJob
{
    public int DoWork(int seed)
    {
        return result = DoWorkAsync(seed).GetAwaiter().GetResult();
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

```

Then we can have a typical deadlock case:

``` cs
var window = new MyWindow();
window.Showed += delegate {
    var myJob = new MyJob();
    myJob.DoWork(10);
};

MyApplication.Run(window);
```

### **DEADLOCK** comes!

Let's run the code snippet above. Please see the whole project in [DispatcherModel](Examples/SynchronizationContextDeadlock/DispatcherModel). (**Note that, to track the deadlock in our topic, I add trace logs at some places additionally.**)

``` log
// post of call to mainWindow.Show
-------------------- Post being called --------------------
   at System.Environment.get_StackTrace()
   at DispatcherModel.MySynchronizationContext.Post(SendOrPostCallback d, Object state) in /workspaces/Dane.Blogs/Techniques/CSharp/Examples/SynchronizationContextDeadlock/DispatcherModel/MySynchronizationContext.cs:line 34
   at DispatcherModel.MyApplication.Run(MyWindow window) in /workspaces/Dane.Blogs/Techniques/CSharp/Examples/SynchronizationContextDeadlock/DispatcherModel/MyApplication.cs:line 19
   at Program.<Main>$(String[] args) in /workspaces/Dane.Blogs/Techniques/CSharp/Examples/SynchronizationContextDeadlock/TypicalCase/Program.cs:line 12
-------------------- Post call exiting --------------------

// message loop starts running
^^^^^^^^^^^^^^^^^^^^ Looking for a job... ^^^^^^^^^^^^^^^^^^^^

// message of call to mainWindow.Show
^^^^^^^^^^^^^^^^^^^^ A job found. Executing... ^^^^^^^^^^^^^^^^^^^^

// handling call to `.GetResult().GetResult()`
******************** Before GetResult call ********************

// It seems that a new message is posted
-------------------- Post being called --------------------
   at System.Environment.get_StackTrace()
   at DispatcherModel.MySynchronizationContext.Post(SendOrPostCallback d, Object state) in /workspaces/Dane.Blogs/Techniques/CSharp/Examples/SynchronizationContextDeadlock/DispatcherModel/MySynchronizationContext.cs:line 34
   at System.Threading.Tasks.AwaitTaskContinuation.RunCallback(ContextCallback callback, Object state, Task& currentTask)
   at System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1.AwaitUnsafeOnCompleted[TAwaiter](TAwaiter& awaiter, IAsyncStateMachineBox box)
   at Workload.MyJob.DoWorkAsync(Int32 seed) in /workspaces/Dane.Blogs/Techniques/CSharp/Examples/SynchronizationContextDeadlock/Workload/MyJob.cs:line 24
   at System.Runtime.CompilerServices.AsyncMethodBuilderCore.Start[TStateMachine](TStateMachine& stateMachine)
   at Workload.MyJob.DoWorkAsync(Int32 seed)
   at Workload.MyJob.DoWork(Int32 seed) in /workspaces/Dane.Blogs/Techniques/CSharp/Examples/SynchronizationContextDeadlock/Workload/MyJob.cs:line 12
   at Program.<>c.<<Main>$>b__0_0(Object <p0>, EventArgs <p1>) in /workspaces/Dane.Blogs/Techniques/CSharp/Examples/SynchronizationContextDeadlock/TypicalCase/Program.cs:line 9
   at DispatcherModel.MyWindow.OnShow() in /workspaces/Dane.Blogs/Techniques/CSharp/Examples/SynchronizationContextDeadlock/DispatcherModel/MyWindow.cs:line 13
   at DispatcherModel.MyWindow.Show() in /workspaces/Dane.Blogs/Techniques/CSharp/Examples/SynchronizationContextDeadlock/DispatcherModel/MyWindow.cs:line 7
   at DispatcherModel.MyApplication.<>c__DisplayClass0_0.<Run>b__0(Object s) in /workspaces/Dane.Blogs/Techniques/CSharp/Examples/SynchronizationContextDeadlock/DispatcherModel/MyApplication.cs:line 21
   at DispatcherModel.MySynchronizationContext.<>c__DisplayClass6_0.<Post>b__0(Object s) in /workspaces/Dane.Blogs/Techniques/CSharp/Examples/SynchronizationContextDeadlock/DispatcherModel/MySynchronizationContext.cs:line 36
   at DispatcherModel.MyApplication.Run(MyWindow window) in /workspaces/Dane.Blogs/Techniques/CSharp/Examples/SynchronizationContextDeadlock/DispatcherModel/MyApplication.cs:line 35
   at Program.<Main>$(String[] args) in /workspaces/Dane.Blogs/Techniques/CSharp/Examples/SynchronizationContextDeadlock/TypicalCase/Program.cs:line 12
-------------------- Post call exiting --------------------
// but it seems that this new message has never been handled by dispatcher
```

The whole process before deadlock is:

1. Main thread begins to handle a message `MessageA` (an action `ActionA`, actually).
2. In the progress of running the `ActionA` above, a new message `MessageB` is posted into the queue by call to **our own**`SynchronizationContext.Post`.
3. `ActionA` waits for `MessageB` to be executed.
4. Main thread is running `ActionA` right now, so it's unable to handle `MessageB` because only when current action is done, main thread then has the chance to query next message.
5. Well, `MessageB` won't never be obtained and executed. Then `ActionA` will wait infinitely. Main thread goes into deadlock.

From the symptom of deadlock issue, there are two questions we can come up with:

1. A message is generated in the process of `.GetResult()`. What's it?
2. Who calls `SynchronizationContext.Post` to send the message?

To answer the two questions above, we need to comprehend the mechanism of [TAP](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/task-asynchronous-programming-model). To make long story short, or in my words, the code after *await call* will be wrapped as a *callback*. This is `MessageB` mentioned above. Once *await call* is done, the *callback* will be scheduled via `SynchronizationContext.Post`, **which can be proved from the preceding logs**. If there is no explicit call to `Task.ConfigureAwait(false)` to suppress capturing original sync context, original `SynchronizationContext` bound to original thread will be captured and this context will be used to call `Post` method on. This is why our own context is used in this case.

## Workarounds

1. DO NOT mix use of sync and async. See [Don't block, await instead](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/#dont-block-await-instead).
2. Avoid capturing current synchronization context before `await` if you're sure what you are calling has nothing to do with UI. (*I mean you have understood the stuff all above. To be honest, I apply the means below only in test.*)
    1. Create a new async method. Wrap the async one we need after a call to `await Task.Delay(1).ConfigureAwait(false);`. See [Solution 1](Examples/SynchronizationContextDeadlock/Solution1/Program.cs). Notes:
        1. There is no need to use `.ConfigureAwait(false)` wherever possible. Only one in UI layer is sufficient enough.
        2. `await Task.Delay(1)` will yield current execution. Apparently, it leads to impact on performance.
    2. Before we call `.GetResult()`, install a default context on current thread and restore after the blocking call is done. See [Solution 2](Examples/SynchronizationContextDeadlock/Solution2/Program.cs).