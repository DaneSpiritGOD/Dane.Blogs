# Let's talk about deadlock caused by /

This kind of issue has been discussed again and again on the Internet. See:

- [is-getawaiter-getresult-safe-for-general-use](https://stackoverflow.com/questions/39007006/is-getawaiter-getresult-safe-for-general-use)
- [dont-block-on-async-code.html](https://blog.stephencleary.com/2012/07/dont-block-on-async-code.html)
- [Why is .GetAwaiter().GetResult() bad in C#?](https://www.nikouusitalo.com/blog/why-is-getawaiter-getresult-bad-in-c/)
- [Understanding Async, Avoiding Deadlocks in C#](https://medium.com/rubrikkgroup/understanding-async-avoiding-deadlocks-e41f8f2c6f5d)

Most of people says `Task.Result` or `Task.GetAwaiter().GetResult()` might cause deadlock at UI-context runtime. UI-context thread is blocked and completion callback inside Task being awaited is waiting for UI context... **But what on earth does such deadlock model look like in code base?**

## What's UI context?

Before I show you my understanding, I wish you to know what is this kind of conext and what it is used for first.

Said by Microsoft, [SynchronizationContext](https://learn.microsoft.com/en-us/dotnet/api/system.threading.synchronizationcontext?view=net-7.0)

> Provides the basic functionality for propagating a synchronization context in various synchronization models.

There is another post written by Stephen Cleary, which introduced history of this class and a serial of demonstrations, comparisons, analysis of implementations in fine details. [It's All About the SynchronizationContext](https://learn.microsoft.com/en-us/archive/msdn-magazine/2011/february/msdn-magazine-parallel-computing-it-s-all-about-the-synchronizationcontext). It's a long read but please keep patient and enjoy it. No one else can do it better.
