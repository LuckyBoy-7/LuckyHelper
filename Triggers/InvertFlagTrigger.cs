using Celeste.Mod.Entities;
using LuckyHelper.Extensions;

namespace LuckyHelper.Triggers;

[CustomEntity("LuckyHelper/InvertFlagTrigger")]
[Tracked]
public class InvertFlagTrigger : FlagTrigger
{
    public InvertFlagTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
    }

    private void InvertFlag()
    {
        Session session = this.Session();
        session.SetFlag(flag, !session.GetFlag(flag));
    }

    public override bool TrySetFlag()
    {
        InvertFlag();
        return true;
    }
}