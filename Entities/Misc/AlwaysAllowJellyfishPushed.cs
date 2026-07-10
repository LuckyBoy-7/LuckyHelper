using Celeste.Mod.Entities;
using LuckyHelper.Module;
using Microsoft.Xna.Framework.Input;
using Glider = On.Celeste.Glider;

namespace LuckyHelper.Entities.Misc;

[CustomEntity("LuckyHelper/AlwaysAllowJellyfishPushed")]
[Tracked]
public class AlwaysAllowJellyfishPushed : Entity
{
    public AlwaysAllowJellyfishPushed(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
    }

    [Load]
    public static void Load()
    {
        On.Celeste.Glider.OnPickup += GliderOnOnPickup;
    }

    [Unload]
    public static void Unload()
    {
        On.Celeste.Glider.OnPickup -= GliderOnOnPickup;
    }

    private static void GliderOnOnPickup(Glider.orig_OnPickup orig, Celeste.Glider self)
    {
        orig(self);
        if (self.level.Tracker.GetEntities<AlwaysAllowJellyfishPushed>().Count > 0)
        {
            self.AllowPushing = true;
        }
    }
}