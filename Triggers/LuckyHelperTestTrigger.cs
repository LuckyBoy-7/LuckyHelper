using Celeste.Mod.Entities;

namespace LuckyHelper.Triggers;

[CustomEntity("LuckyHelper/LuckyHelperTestTrigger")]
[Tracked]
public class LuckyHelperTestTrigger : Trigger
{
    public LuckyHelperTestTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
    }

    public override void OnLeave(Player player)
    {
        base.OnEnter(player);
    }
}