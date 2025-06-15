using System.Reflection;
using LuckyHelper.Extensions;
using LuckyHelper.Module;
using LuckyHelper.Triggers;
using LuckyHelper.Utils;
using Microsoft.Xna.Framework.Input;
using Mono.Cecil.Cil;
using MonoMod.RuntimeDetour;
using Level = On.Celeste.Level;
using Player = Celeste.Player;
using WindController = On.Celeste.WindController;

namespace LuckyHelper.Modules;

public class LoadLevelModule
{
    [Load]
    public static void Load()
    {
        On.Celeste.Level.LoadLevel += LevelOnLoadLevel;
    }

    private static void LevelOnLoadLevel(Level.orig_LoadLevel orig, Celeste.Level self, Player.IntroTypes playerintro, bool isfromloader)
    {
        OverlapPairSetFlagTrigger.IdToTriggerSet.Clear();
        TalkComponentController.TalkComponents.Clear();
        orig(self, playerintro, isfromloader);
    }

    [Unload]
    public static void Unload()
    {
        On.Celeste.Level.LoadLevel -= LevelOnLoadLevel;
    }
}