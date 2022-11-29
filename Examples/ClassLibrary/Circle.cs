namespace ClassLibrary
{
    public readonly struct Circle
    {
        public readonly double r;
        public Circle(double r)
        {
            this.r = r;
        }
        public double Area => 3.1415926535 * r * r;
        public double Circumference => 2 * 3.1415926535 * r;

    }
}
