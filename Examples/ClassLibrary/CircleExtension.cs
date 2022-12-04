namespace ClassLibrary;
public static class CircleExtension
{
    public static double Circumference(this double v) => new Circle(v).Circumference;
    public static double Area(this double v) => new Circle(v).Area;
}
