// See https://aka.ms/new-console-template for more information

var r = new RefType
{
    A = 1,
};
r.SetB(2);

var v = new ValType
{
    A = 8,
};
v.SetB(10);

Console.WriteLine($"--- Original ---");
Console.WriteLine($"Reference Obj: A = {r.A}, B = {r.B}");
Console.WriteLine($"Value Obj: A = {v.A}, B = {v.B}");

var mix = new Mix()
{
    Ref = r,
    Val = v,
};

mix.Ref.A = 11;
mix.Ref.SetB(20);

// The line below gets a compiler error:
// error CS1612: Cannot modify the return value of 'Mix.Value' because it is not a variable
// mix.Val.A = 888;
mix.Val.SetB(1000);

Console.WriteLine($"{Environment.NewLine}--- Set value-type data by referencing data directly ---");
Console.WriteLine($"Reference Obj: A = {r.A}, B = {r.B}");
Console.WriteLine($"Mix.Reference Obj: A = {mix.Ref.A}, B = {mix.Ref.B}");
Console.WriteLine($"Value Obj: A = {v.A}, B = {v.B}");
Console.WriteLine($"Mix.Value Obj: A = {mix.Val.A}, B = {mix.Val.B}");

mix.SetValueA(8888);
mix.SetValueB(10000);

Console.WriteLine($"{Environment.NewLine}--- Set value-type data by modifying the variable of host ---");
Console.WriteLine($"Value Obj: A = {v.A}, B = {v.B}");
Console.WriteLine($"Mix.Value Obj: A = {mix.Val.A}, B = {mix.Val.B}");

ref var vv = ref mix.GetValue();
vv.A = 9999;
vv.SetB(5000);

Console.WriteLine($"{Environment.NewLine}--- Set value-type data by ref return ---");
Console.WriteLine($"Value Obj: A = {v.A}, B = {v.B}");
Console.WriteLine($"Mix.Value Obj: A = {mix.Val.A}, B = {mix.Val.B}");

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

class Mix
{
    public RefType? Ref { get; set; }

    ValType val;
    public ValType Val
    {
        get => val;
        set => val = value;
    }

    public void SetValueA(int a) => val.A = a;
    public void SetValueB(int b) => val.SetB(b);

    public ref ValType GetValue() => ref val;
}