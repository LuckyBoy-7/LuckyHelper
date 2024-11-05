namespace LuckyHelper.Extensions;

public static class ColorExtensions
{
    public static Color WithA(this Color color, float a)
    {
        return new Color(color.R, color.G, color.B, a);
    }
}