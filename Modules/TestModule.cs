#define rect
#define tmpA
#define strength
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
        On.Celeste.Bumper.Update += BumperOnUpdate;
        On.Celeste.Bumper.Added += BumperOnAdded;
    }

    private static void BumperOnAdded(Bumper.orig_Added orig, Celeste.Bumper self, Scene scene)
    {
        orig(self, scene);
        if (self.Components.FirstOrDefault(component => component is Tween) is Tween t)
        {
            t.Duration = 0.2f;
            t.Reset();
        }
    }

    private static void BumperOnUpdate(Bumper.orig_Update orig, Celeste.Bumper self)
    {
        orig(self);
        // if (self.Components.FirstOrDefault(component => component is Tween) is Tween t)
        // {
        //     t.Duration = 0.2f;
        // }
        // for (int _ = 0; _ < 5; _++)
        // {
        //     self.sine.Update();
        //     self.UpdatePosition();
        // }
    }


    [Unload]
    public static void Unload()
    {
        On.Celeste.Bumper.Update -= BumperOnUpdate;
        On.Celeste.Bumper.Added -= BumperOnAdded;
    }
}