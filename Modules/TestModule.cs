#if DEBUG

using LuckyHelper.Components;
using LuckyHelper.Module;
using LuckyHelper.Utils;
using Level = On.Celeste.Level;


namespace LuckyHelper.Modules;

public static class TestModule
{
    [Load]
    public static void Load()
    {
        On.Celeste.Level.Update += LevelOnUpdate;
        On.Celeste.Level.UnloadLevel += LevelOnUnloadLevel;
    }


    [Unload]
    public static void Unload()
    {
        On.Celeste.Level.Update -= LevelOnUpdate;
        On.Celeste.Level.UnloadLevel -= LevelOnUnloadLevel;
    }

    private static void LevelOnUnloadLevel(Level.orig_UnloadLevel orig, Celeste.Level self)
    {
        orig(self);
    }

    private static void LevelOnUpdate(Level.orig_Update orig, Celeste.Level self)
    {
        orig(self);
    }
}
#endif