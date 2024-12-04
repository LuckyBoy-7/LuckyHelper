using System.Collections;
using System.Runtime.CompilerServices;
using Celeste.Mod.Entities;
using LuckyHelper.Extensions;

namespace LuckyHelper.Entities.LootSpeedrun;

[CustomEntity("LuckyHelper/LootThresholdFlagSet")]
[Tracked]
public class LootThresholdFlagSet : Trigger
{
    private int threshold;
    private string flag;

    public LootThresholdFlagSet(EntityData data, Vector2 offset) : base(data, offset)
    {
        threshold = data.Int("threshold");
        flag = data.Attr("flag");
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        if (this.Session().GetCounter(LootSpeedrunController.LootMaxValueID) >= threshold)
            this.Session().SetFlag(flag);
    }
}