using Celeste.Mod.Entities;
using LuckyHelper.Extensions;
using LuckyHelper.Utils;

namespace LuckyHelper.Triggers;

[CustomEntity("LuckyHelper/InvertFlagTrigger")]
[Tracked]
public class InvertFlagTrigger : EntityTrigger
{
    private string flag;
    private bool invertOnEnter;
    private bool invertOnLeave;

    public InvertFlagTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        flag = data.Attr("flag");
        invertOnEnter = data.Bool("invertOnEnter");
        invertOnLeave = data.Bool("invertOnLeave");
    }

    public override void OnCustomEnter(Entity entity)
    {
        if (invertOnEnter)
            InvertFlag();
    }

    private void InvertFlag()
    {
        Session session = this.Session();
        session.SetFlag(flag, !session.GetFlag(flag));
    }

    public override void OnCustomLeave(Entity entity)
    {
        if (invertOnLeave)
            InvertFlag();
    }

    public override void OnCustomStay(Entity entity)
    {
    }
}