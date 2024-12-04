using Celeste.Mod.Entities;
using LuckyHelper.Extensions;
using Microsoft.Xna.Framework.Input;

namespace LuckyHelper.Entities.LootSpeedrun;

[CustomEntity("LuckyHelper/LootSpeedrunStartup")]
[Tracked]
public class LootSpeedrunStartup : Trigger
{
    // 启动时触发的flag
    private string flagOnStart = "";

    public LootSpeedrunStartup(EntityData data, Vector2 offset) : base(data, offset)
    {
        flagOnStart = data.Attr("flagOnStart");
    }


    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        this.Session().SetFlag(flagOnStart);
        // 启动!!!
        this.GetEntity<LootSpeedrunController>().TryStart();
    }
}