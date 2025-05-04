namespace LuckyHelper.Extensions;

public static class ColorExtensions
{
    public static Color WithA(this Color color, byte a)
    {
        color.A = a;
        return color;
    }

    public static Color WithA(this Color color, float a)
    {
        return new Color(color, a);
    }
}