using Lucky.Kits.Collections;
using LuckyHelper.Handlers;
using LuckyHelper.Handlers.Impl;
using LuckyHelper.Modules;
using LuckyHelper.Utils;
using Microsoft.Xna.Framework.Graphics;
using Level = On.Celeste.Level;

namespace LuckyHelper.Module;

public class LuckyHelperBuffers
{
    public static VirtualRenderTarget TempC;


    [Load]
    public static void Load()
    {
        On.Celeste.Level.Begin += LevelOnBegin;
    }

    [Unload]
    public static void Unload()
    {
        On.Celeste.Level.Begin -= LevelOnBegin;
    }

    private static void LevelOnBegin(Level.orig_Begin orig, Celeste.Level self)
    {
        orig(self);
        var tempA = GameplayBuffers.TempA;
        TempC = GameplayBuffers.Create(tempA.Width, tempA.Height);
    }
}