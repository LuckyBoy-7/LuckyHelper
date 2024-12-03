using Celeste.Mod.Entities;
using LuckyHelper.Extensions;
using Microsoft.Xna.Framework.Input;

namespace LuckyHelper.Entities.LootSpeedrun;

[CustomEntity("LuckyHelper/LootSpeedrunStartup")]
[Tracked]
public class LootSpeedrunStartup : Trigger
{
    public const string LootRestartID = "LootRestart";
    private string startRoom = "";

    public LootSpeedrunStartup(EntityData data, Vector2 offset) : base(data, offset)
    {
        startRoom = data.Attr("startRoom");
    }


    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        Vector2 pos = this.Session().MapData.Get(startRoom).Spawns[0];
        player.Position = pos;
        this.Session().SetFlag(LootRestartID);
    }
}