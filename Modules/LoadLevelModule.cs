using System.Reflection;
using LuckyHelper.Extensions;
using LuckyHelper.Module;
using LuckyHelper.Triggers;
using LuckyHelper.Utils;
using Microsoft.Xna.Framework.Input;
using Mono.Cecil.Cil;
using MonoMod.RuntimeDetour;
using WindController = On.Celeste.WindController;

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
        OverlapPairSetFlagTrigger.IdToTriggerSet.Clear();
        TypeToObjectsModule.BriefTypeToEntities.Clear();
        TypeToObjectsModule.BriefTypeToComponents.Clear();

        LuckyHelperModule.Session.LuckyHelperAreaMetadata = LuckyHelperAreaMetadata.TryGetCameraMetadata(level.Session);
        orig(level, playerintro, isfromloader);
    }

    [Unload]
    public static void Unload()
    {
        On.Celeste.Level.LoadLevel -= LevelOnLoadLevel;
    }
}