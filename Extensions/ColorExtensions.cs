namespace LuckyHelper.Extensions;

internal static class ColorExtensions
{
    public static Color Multiply(this Color color, Color other)
    {
        color.R = (byte)(color.R * other.R / 255);
        color.G = (byte)(color.G * other.G / 255);
        color.B = (byte)(color.B * other.B / 255);
        color.A = (byte)(color.A * other.A / 255);
        return color;
    }
}