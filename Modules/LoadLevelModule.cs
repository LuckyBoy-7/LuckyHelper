using LuckyHelper.Module;
using LuckyHelper.Triggers;

namespace LuckyHelper.Modules;

public class LoadLevelModule
{
    [Load]
    public static void Load()
    {
        On.Celeste.Level.LoadLevel += LevelOnLoadLevel;
    }

    private static void LevelOnLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Celeste.Level level, Player.IntroTypes playerintro, bool isfromloader)
    {
        LuckyHelperModule.Session.LuckyHelperAreaMetadata = LuckyHelperAreaMetadata.TryGetMetadata(level.Session);
        orig(level, playerintro, isfromloader);
    }

    [Unload]
    public static void Unload()
    {
        On.Celeste.Level.LoadLevel -= LevelOnLoadLevel;
    }
}