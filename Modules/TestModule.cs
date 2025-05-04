using System.Reflection;
using LuckyHelper.Extensions;
using LuckyHelper.Module;
using LuckyHelper.Utils;
using Mono.Cecil.Cil;
using MonoMod.RuntimeDetour;
using Player = Celeste.Player;
using WindController = On.Celeste.WindController;

namespace LuckyHelper.Modules;

public class TestModule
{
    [Load]
    public static void Load()
    {
        // PlayerFallOnPlatform_ILHook = new(typeof(Player).GetMethod(
        //     "orig_Update"
        // ), PlayerOrig_UpdateHook);
    }

    [Unload]
    public static void Unload()
    {
    }


}