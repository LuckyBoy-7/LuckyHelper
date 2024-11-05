using System.Reflection;
using LuckyHelper.Module;
using Microsoft.Xna.Framework.Input;
using Player = On.Celeste.Player;
using SwapBlock = On.Celeste.SwapBlock;

namespace LuckyHelper.Modules;

public class PlayerFallThroughJumpThruModule
{
    [Load]
    public static void Load()
    {
        On.Celeste.Player.Update += PlayerOnUpdate;
    }


    private static void PlayerOnUpdate(Player.orig_Update orig, Celeste.Player self)
    {
        orig(self);
        if (!LuckyHelperModule.Settings.EnablePlayerFallingThroughJumpThru)
            return;

        // 按住下并且按了空格
        if (Input.MoveY.Value > 0 && LuckyHelperModule.Settings.PlayerFallingThroughJumpThruButton.Check)
        {
            // 下面是单向板
            if (self.Scene.Tracker.GetEntities<JumpThru>().Any(jump => self.IsRiding((JumpThru)jump)))
            {
                // 且没有solid
                if (self.Scene.Tracker.GetEntities<Solid>().Any(solid => self.IsRiding((Solid)solid)))
                    return;
                self.NaiveMove(new Vector2(0, 1));
            }
        }
    }


    [Unload]
    public static void Unload()
    {
        On.Celeste.Player.Update -= PlayerOnUpdate;
    }
}