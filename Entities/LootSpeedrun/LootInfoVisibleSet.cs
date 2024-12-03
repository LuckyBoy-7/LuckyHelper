using System.Collections;
using System.Runtime.CompilerServices;
using Celeste.Mod.Entities;
using LuckyHelper.Extensions;

namespace LuckyHelper.Entities.LootSpeedrun;

[CustomEntity("LuckyHelper/LootInfoVisibleSet")]
[Tracked]
public class LootInfoVisibleSet : Trigger
{
    public bool visible;

    public LootInfoVisibleSet(EntityData data, Vector2 offset) : base(data, offset)
    {
        visible = data.Bool("visible");
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        LootSpeedrunInfoDisplay display = null;
        if (this.GetEntity<LootSpeedrunInfoDisplay>() != null)
            display = this.GetEntity<LootSpeedrunInfoDisplay>();
        else
            Scene.Add(display = new LootSpeedrunInfoDisplay());
        
        if (visible)
            display?.Show();
        else
            display?.Hide();
    }
}