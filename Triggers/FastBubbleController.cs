using Celeste.Mod.Entities;
using LuckyHelper.Module;

namespace LuckyHelper.Triggers;

[CustomEntity("LuckyHelper/FastBubbleController")]
[Tracked]
public class FastBubbleController : Trigger
{
    private bool disableFastBubble;
    public FastBubbleController(EntityData data, Vector2 offset) : base(data, offset)
    {
        disableFastBubble = data.Bool("disableFastBubble");
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        LuckyHelperModule.Session.DisableFastBubble = disableFastBubble;
    }
}