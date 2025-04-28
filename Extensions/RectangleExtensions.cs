namespace LuckyHelper.Extensions;

public static class RectangleExtensions
{
    public static Vector2 SizeDelta(this Rectangle rectangle)
    {
        return new Vector2(rectangle.Width, rectangle.Height);
    }
    
    public static Vector2 Position(this Rectangle rectangle)
    {
        return new Vector2(rectangle.X, rectangle.Y);
    }
}