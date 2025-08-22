using System.Reflection;
using Celeste.Mod.Entities;
using LuckyHelper.Module;
using LuckyHelper.Modules;
using LuckyHelper.Utils;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace LuckyHelper.Triggers;

[CustomEntity("LuckyHelper/ExCameraUseUnsafeZoom")]
[Tracked]
public class ExCameraUseUnsafeZoom : Trigger
{
    public bool Use;


    public ExCameraUseUnsafeZoom(EntityData data, Vector2 offset) : base(data, offset)
    {
        Use = data.Bool("use");
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        LuckyHelperModule.Session.UseUnsafeZoom = Use;
    }

    public static ILHook UnsafeZoomHook;

    [Load]
    public static void Load()
    {
        if (ModCompatModule.ExCameraDynamicsLoaded)
        {
            Type moduleType = Everest.Modules.First(mod => mod.GetType().ToString() == "Celeste.Mod.ExCameraDynamics.ExCameraModule").GetType();
            Type classType = moduleType.Assembly.GetType("Celeste.Mod.ExCameraDynamics.CalcPlus");
            UnsafeZoomHook = new ILHook(classType.GetMethod(
                "GetCameraZoomBounds",
                BindingFlags.Static | BindingFlags.Public,
                new[] { typeof(Player), typeof(Level), typeof(bool) }
            ), OnUnsafeZoom);
        }
    }

    private static void OnUnsafeZoom(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);

        if (cursor.TryGotoNext(ins => ins.MatchLdarg(0),
                ins => ins.MatchCallvirt(typeof(Player).GetProperty("InControl").GetGetMethod())
            ))
        {
            ILLabel outLabel = cursor.DefineLabel();
            cursor.Index += 1;
            cursor.EmitPop();
            cursor.EmitDelegate<Func<bool>>(() =>
            {
                LuckyHelperSession session = LuckyHelperModule.Session;
                return session.UseUnsafeZoom;
            });
            cursor.EmitBrtrue(outLabel);
            cursor.EmitLdarg0();

            if (
                cursor.TryGotoNext(
                    ins => ins.MatchLdarg(0),
                    ins => ins.MatchLdarg(1),
                    ins => ins.MatchLdarg(2)
                ))
            {
                cursor.MarkLabel(outLabel);
            }

            // cursor.Index = 10;
            // cursor.CILCodeLogger(100);
        }
    }


    [Unload]
    public static void Unload()
    {
        if (ModCompatModule.ExCameraDynamicsLoaded)
        {
            UnsafeZoomHook?.Dispose();
            UnsafeZoomHook = null;
        }
    }
}