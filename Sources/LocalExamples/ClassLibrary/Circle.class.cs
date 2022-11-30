namespace ClassLibrary;

public readonly struct Circle
{
    public readonly double R;
    public Circle(double r)
    {
        this.R = r;
    }
    public double Area => 3.1415926535 * R * R;
    public double Circumference => 2 * 3.1415926535 * R;
    public override string ToString() => $"r={R}";
}