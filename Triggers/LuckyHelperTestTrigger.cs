#if DEBUG
using Celeste.Mod.Entities;
using LuckyHelper.Utils;

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

    public override void OnStay(Player player)
    {
        base.OnStay(player);
        // player.level.Camera.UpdateMatrices();
    }

    public override void OnLeave(Player player)
    {
        base.OnEnter(player);
    }
}
#endif