namespace LuckyHelper.Utils;

public static class MathUtils
{
    public static bool Between(float val, float a, float b)
    {
        if (b < a)
            (a, b) = (b, a);
        return val >= a && val <= b;
    }

}