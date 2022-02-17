// https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/try-finally

AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;

// Method1();
// Method2();

try
{
    Method1();
}
catch
{
    Console.WriteLine("Method1 wrapper: Exception is caught by outer catch block.");
}

// try
// {
//     Method2();
// }
// catch
// {
//     Console.WriteLine("Method2 wrapper: Exception is caught by outer catch block.");
// }

static void Method1()
{
    try
    {
        Console.WriteLine("Method1: try");
        throw new Exception();
    }
    catch
    {
        Console.WriteLine("Method1: catch");
        throw new Exception();
    }
    finally
    {
        Console.WriteLine("Method1: I'm in finally block.");
    }
}

static void Method2()
{
    try
    {
        try
        {
            Console.WriteLine("Method2: try");
            throw new Exception();
        }
        catch
        {
            Console.WriteLine("Method2: catch");
            throw new Exception();
        }
    }
    finally
    {
        Console.WriteLine("Method2: I'm in finally block.");
    }
}

 static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
 {
    Console.WriteLine("I'm in exception handler.");
 }
