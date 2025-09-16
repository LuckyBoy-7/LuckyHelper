using LuckyHelper.Modules;

namespace LuckyHelper.Extensions;

public static class IntExtensions
{
    public static int Mod(this int x, int y)
    {
        return (x % y + y) % y;
    }
}