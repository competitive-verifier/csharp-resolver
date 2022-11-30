namespace ClassLibrary;

public record struct Circle(double R)
{
    public double Area => 3.1415926535 * R * R;
    public double Circumference => 2 * 3.1415926535 * R;
    public override string ToString() => $"r={R}";
}