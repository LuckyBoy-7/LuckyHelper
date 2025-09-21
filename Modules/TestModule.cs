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
using PlayerCollider = On.Celeste.PlayerCollider;


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