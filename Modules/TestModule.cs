#if DEBUG

using LuckyHelper.Module;


namespace LuckyHelper.Modules;

public static class TestModule
{
    [Load]
    public static void Load()
    {
    }


    [Unload]
    public static void Unload()
    {
    }
}
#endif