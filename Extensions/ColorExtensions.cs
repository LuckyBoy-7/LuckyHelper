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

    public static Color Approach(this Color color, Color targetColor, float speed)
    {
        if (speed == 0)
            return color;
        speed = Math.Max(speed, 1);
        color.R = (byte)Calc.Approach(color.R, targetColor.R, speed);
        color.G = (byte)Calc.Approach(color.G, targetColor.G, speed);
        color.B = (byte)Calc.Approach(color.B, targetColor.B, speed);
        color.A = (byte)Calc.Approach(color.A, targetColor.A, speed);
        return color;
    }
}