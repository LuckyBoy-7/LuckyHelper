using Celeste.Mod.Entities;

namespace LuckyHelper.Triggers;

[CustomEntity("LuckyHelper/FreezeTrigger")]
[Tracked]
public class FreezeTrigger : Trigger
{
    public float FreezeTime;
    
    public FreezeTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        FreezeTime = data.Float("freezeTime");
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        Celeste.Celeste.Freeze(FreezeTime);
    }
}