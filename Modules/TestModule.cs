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
        
        // On.Celeste.Player.Render += PlayerOnRender;
    }

    private static void PlayerOnRender(On.Celeste.Player.orig_Render orig, Player self)
    {
        // orig(self);
        // self.Hair.Sprite
        // self.Hair.Render();
    }

    [Unload]
    public static void Unload()
    {
        // On.Celeste.Player.Render -= PlayerOnRender;
    }
}