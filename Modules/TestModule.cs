#if DEBUG

using System.Collections;
using LuckyHelper.Components;
using LuckyHelper.Entities;
using LuckyHelper.Module;
using LuckyHelper.Utils;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;


namespace LuckyHelper.Modules;

public static class TestModule
{
    private static ILHook hook;
    [Load]
    public static void Load()
    {
         // hook = new ILHook(typeof(Level).GetMethod("orig_LoadLevel"), OnOrigLoadLevel);
    }

    private static void OnOrigLoadLevel(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        if (cursor.TryGotoNext(
                ins => ins.MatchLdarg0(),
                ins => ins.MatchLdcR4(1f),
                ins => ins.MatchStfld(typeof(Level).GetField("Zoom"))
            ))
        {
            cursor.Index += 2;
            cursor.EmitLdarg0();
            cursor.EmitDelegate<Func<float, Level, float>>((newZoom, level) => { return level.Zoom; });
        }
    }


    [Unload]
    public static void Unload()
    {
        hook?.Dispose();
        hook = null;
    }
}
#endif