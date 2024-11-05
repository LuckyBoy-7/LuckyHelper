using System.Reflection;
using LuckyHelper.Module;
using Microsoft.Xna.Framework.Input;
using Player = On.Celeste.Player;
using SwapBlock = On.Celeste.SwapBlock;


namespace LuckyHelper.Modules;
public class PlayerPixelMoveDebugModule
{
    [Load]
    public static void Load()
    {
        On.Celeste.Player.Update += PlayerOnUpdate;
    }


    private static void PlayerOnUpdate(Player.orig_Update orig, Celeste.Player self)
    {
        orig(self);
        if (!LuckyHelperModule.Settings.IsDebugging)
            return;
        // 按住下并且按了空格
        if (MInput.Keyboard.Check(Keys.LeftAlt))
        {
            if (MInput.Keyboard.Pressed(Keys.Right))
                self.Position += new Vector2(1, 0);
            if (MInput.Keyboard.Pressed(Keys.Left))
                self.Position += new Vector2(-1, 0);
            if (MInput.Keyboard.Pressed(Keys.Up))
                self.Position += new Vector2(0, -1);
            if (MInput.Keyboard.Pressed(Keys.Down))
                self.Position += new Vector2(0, 1);
            
        }
    }


    [Unload]
    public static void Unload()
    {
        On.Celeste.Player.Update -= PlayerOnUpdate;
    }
}