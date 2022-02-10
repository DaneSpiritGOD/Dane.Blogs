// See https://aka.ms/new-console-template for more information

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

Console.WriteLine($"Reference Obj: A = {r.A}, B = {r.B}");
Console.WriteLine($"Value Obj: A = {v.A}, B = {v.B}");

var mix = new Mix()
{
    Ref = r,
    Val = v,
};

mix.Ref.A = 88;
mix.Ref.SetB(999);

// The line below gets a compiler error:
// error CS1612: Cannot modify the return value of 'Mix.Value' because it is not a variable
// mix.Val.A = 888;
mix.Val.SetB(9999);

Console.WriteLine($"{Environment.NewLine}--- After Mix - 1 ---");
Console.WriteLine($"Reference Obj: A = {r.A}, B = {r.B}");
Console.WriteLine($"Value Obj: A = {v.A}, B = {v.B}");
Console.WriteLine($"Mix.Reference Obj: A = {mix.Ref.A}, B = {mix.Ref.B}");
Console.WriteLine($"Mix.Value Obj: A = {mix.Val.A}, B = {mix.Val.B}");

ref var vv = ref mix.GetValue();
vv.A = 888;
vv.SetB(9999);

Console.WriteLine($"{Environment.NewLine}--- After Mix - 2 ---");
Console.WriteLine($"Reference Obj: A = {r.A}, B = {r.B}");
Console.WriteLine($"Value Obj: A = {v.A}, B = {v.B}");
Console.WriteLine($"Mix.Reference Obj: A = {mix.Ref.A}, B = {mix.Ref.B}");
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

    public ref ValType GetValue() => ref val;
}