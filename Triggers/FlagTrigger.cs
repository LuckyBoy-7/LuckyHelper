using Celeste.Mod.Entities;
using LuckyHelper.Extensions;
using LuckyHelper.Module;
using LuckyHelper.Utils;

namespace LuckyHelper.Triggers;



public abstract class FlagTrigger : EntityTrigger
{
    protected string flag;
    protected FlagTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        flag = data.Attr("flag");
    }

    public override void OnTriggered()
    {
        TrySetFlag();
    }

    public abstract bool TrySetFlag();
}