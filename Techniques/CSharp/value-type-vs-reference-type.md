# Official Definition
As [*Microsoft Docs* says](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/value-types),
> *Value types* and reference types are the two main categories of C# types. A variable of a value type contains an instance of the type. This differs from a variable of a reference type, which contains a reference to an instance of the type. By default, on assignment, passing an argument to a method, and returning a method result, variable values are copied. In the case of value-type variables, the corresponding type instances are copied.

# Illustrate
Let's look at the code below, what does the two variables' memory layout look like?
``` cs
class RefType
{
    public int A { get; set; }

    int b;
    public int B => b;

    public void SetB(int value) => b = value;
}

struct ValType
{
    public int A { get; set; }

    int b;
    public int B => b;

    public void SetB(int value) => b = value;
}

var r = new RefType
{
    A = 8,
};
r.SetB(10);

var v = new ValType
{
    A = 80,
};
v.SetB(100);
```
![difference-in-memory-between-value-and-reference.png](../../Resources/Images/difference-in-memory-between-value-and-reference.png)

# Advanced
Let's take a look at the code snippet in [Program.cs](Examples/RefValue/Program.cs). The program prints the following output:
> --- Original ---  
> Reference Obj: A = 1, B = 2  
> Value Obj: A = 8, B = 10  
>   
> --- Set value-type data by referencing data directly ---  
> Reference Obj: A = 11, B = 20  
> Mix.Reference Obj: A = 11, B = 20  
> Value Obj: A = 8, B = 10  
> Mix.Value Obj: A = 8, B = 10  
>   
> --- Set value-type data by modifying the variable of host ---  
> Value Obj: A = 8, B = 10  
> Mix.Value Obj: A = 8888, B = 10000  
>   
> --- Set value-type data by ref return ---  
> Value Obj: A = 8, B = 10  
> Mix.Value Obj: A = 9999, B = 5000  

As we can see in the output results, we are even unable to modify the value directly by `mix.Val.A = 888;` or `mix.Val.SetB(1000);` because `mix.Value` returns a **copied value** and the new value copied is not defined as a variable. Then we can't set member to that new value.  
If we really want to revise the data in that value-type value, we have to grant access to the variable that hosts the value. Only operations on the variable can take effects on the value. So, *to make your code less error-prone and more robust, define and use immutable value types.*