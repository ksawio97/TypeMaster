namespace TypeMaster.DataModels;

public class Vector2
{
    public double X { get; set; }
    public double Y { get; set; }

    public Vector2(double X = 0, double Y = 0)
    {
        this.X = X;
        this.Y = Y;
    }
}