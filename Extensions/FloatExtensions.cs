using LuckyHelper.Modules;

namespace LuckyHelper.Extensions;

public static class FloatExtensions
{
    public static float Mod(this float x, float y)
    {
        return (x % y + y) % y;
    }
    
    public static double Mod(this double x, double y)
    {
        return (x % y + y) % y;
    }
}