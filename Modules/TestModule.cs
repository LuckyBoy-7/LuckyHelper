#if DEBUG

using System.Collections;
using LuckyHelper.Components;
using LuckyHelper.Module;
using LuckyHelper.Utils;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace LuckyHelper.Modules;

public static class TestModule
{
    [Load]
    public static void Load()
    {
        On.Celeste.Player.Update += PlayerOnUpdate;
    }

    private static void PlayerOnUpdate(On.Celeste.Player.orig_Update orig, Celeste.Player self)
    {
        // if (MInput.Keyboard.Check(Keys.Y))
            // self.StateMachine.State = Player.StDummy;
        // else
            // self.StateMachine.State = Player.StNormal;
        if (MInput.Keyboard.Pressed(Keys.U))
        {
            // self.Add(new Coroutine(Test(self)));
        }

        if (MInput.Keyboard.Pressed(Keys.I))
        {
            // self.Add(new Coroutine(Test1(self)));
        }

        if (MInput.Keyboard.Check(Keys.U))
        {
        }

        if (MInput.Keyboard.Check(Keys.I))
        {
        }

        orig(self);
    }

    public static IEnumerator Test(Player player)
    {
        Level level = player.level;
        // yield return level.ZoomTo(new Vector2(160, 90), 2, 2);
        yield return level.ZoomTo(new Vector2(320, 180), 2, 2);
    }

    public static IEnumerator Test1(Player player)
    {
        Level level = player.level;
        yield return level.ZoomTo(new Vector2(160, 90), 1, 2);
    }

    [Unload]
    public static void Unload()
    {
        On.Celeste.Player.Update -= PlayerOnUpdate;
    }
}
#endif