using LuckyHelper.Extensions;

namespace LuckyHelper.Utils;

public static class MathUtils
{
    public static bool Between(float val, float a, float b)
    {
        if (b < a)
            (a, b) = (b, a);
        return val >= a && val <= b;
    }

    public static float WrapMod(float val, float min, float max)
    {
        return min + (val - min).Mod(max - min);
    }

    public static double WrapMod(double val, double min, double max)
    {
        return min + (val - min).Mod(max - min);
    }
}