using System.Reflection;
using LuckyHelper.Extensions;
using LuckyHelper.Module;
using LuckyHelper.Utils;
using Microsoft.Xna.Framework.Input;
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
        On.Celeste.Player.Update += PlayerOnUpdate;
    }

    private static Session session;

    private static void PlayerOnUpdate(On.Celeste.Player.orig_Update orig, Player self)
    {
        orig(self);

        // if (MInput.Keyboard.Check(Keys.T))
        // {
        //     session = self.Session();
        //     // LuckyHelperModule.SaveData.session = session;
        //     LuckyHelperModule.SaveData.session = session;
        // }
        //
        // if (MInput.Keyboard.Check(Keys.Y))
        // {
        //     Engine.Scene = new LevelExit(LevelExit.Mode.GoldenBerryRestart, session, null)
        //     {
        //         GoldenStrawberryEntryLevel = "DecalRegistry"
        //     };
        // }
        //
        // if (MInput.Keyboard.Check(Keys.Enter))
        // {
        //     Logger.Warn("Test", self.Session().Area.SID);
        // }
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
        On.Celeste.Player.Update -= PlayerOnUpdate;
        // On.Celeste.Player.Render -= PlayerOnRender;
    }
}