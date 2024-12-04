using Celeste.Mod.Entities;
using LuckyHelper.Extensions;

namespace LuckyHelper.Entities.LootSpeedrun;

[CustomEntity("LuckyHelper/LootMaxValueText")]
public class LootMaxValueText : LuckyText
{
    public override string Content => this.Session().GetCounter(LootSpeedrunController.LootMaxValueID).ToString();

    public LootMaxValueText(EntityData data, Vector2 offset) : base(data, offset)
    {
        Tag = Tags.HUD | Tags.TransitionUpdate;
    }
}