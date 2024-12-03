using Celeste.Mod.Entities;

namespace LuckyHelper.Entities.LootSpeedrun;

[CustomEntity("LuckyHelper/LootCollectArea")]
[Tracked]
public class LootCollectArea : Trigger
{
    public bool CollectOnGround;

    public LootCollectArea(EntityData data, Vector2 offset) : base(data, offset)
    {
        CollectOnGround = data.Bool("collectOnGround");
    }
}