using System.Reflection;
using Celeste.Mod.Entities;
using LuckyHelper.Module;
using MonoMod.RuntimeDetour;

namespace LuckyHelper.Triggers;

[CustomEntity("LuckyHelper/ForceGrabZone")]
[Tracked]
public class ForceGrabZone : Trigger
{
    private string disableFlag;

    public ForceGrabZone(EntityData data, Vector2 offset) : base(data, offset)
    {
        disableFlag = data.Attr("disableFlag");
    }

    private static Hook grabCheckHook;

    private delegate bool GrabCheckGetterDelegate();

    [Load]
    public static void Load()
    {
        var grabCheckGetter = typeof(Input).GetProperty("GrabCheck", BindingFlags.Static | BindingFlags.Public).GetGetMethod();
        grabCheckHook = new Hook(grabCheckGetter, GrabCheckOnGet);
    }

    private static bool GrabCheckOnGet(GrabCheckGetterDelegate orig)
    {
        if (Engine.Scene is Level level)
        {
            foreach (ForceGrabZone forceGrabZone in level.Tracker.GetEntities<ForceGrabZone>())
            {
                if (forceGrabZone.CollideCheck<Player>() && !level.Session.GetFlag(forceGrabZone.disableFlag))
                {
                    return true;
                }
            }
        }
        return orig();
    }


    [Unload]
    public static void Unload()
    {
        grabCheckHook?.Dispose();
        grabCheckHook = null;
    }
}