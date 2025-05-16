namespace LuckyHelper.Extensions;

public static class Vector2Extensions
{
    public static Vector2 WithX(this Vector2 vec, float x)
    {
        vec.X = x;
        return vec;
    }

    public static Vector2 WithY(this Vector2 vec, float y)
    {
        vec.Y = y;
        return vec;
    }
}