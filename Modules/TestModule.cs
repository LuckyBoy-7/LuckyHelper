#if DEBUG

using Lucky.Kits.Collections;
using LuckyHelper.Extensions;
using LuckyHelper.Module;
using Microsoft.Xna.Framework.Graphics;
using Bumper = On.Celeste.Bumper;
using Level = On.Celeste.Level;
using LightingRenderer = On.Celeste.LightingRenderer;


namespace LuckyHelper.Modules;

public static class TestModule
{
    [Load]
    public static void Load()
    {
    }


    [Unload]
    public static void Unload()
    {
    }
}
#endif