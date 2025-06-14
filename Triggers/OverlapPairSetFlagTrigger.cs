using Celeste.Mod.Entities;
using LuckyHelper.Extensions;

namespace LuckyHelper.Triggers;

[CustomEntity("LuckyHelper/OverlapPairSetFlagTrigger")]
[Tracked]
public class OverlapPairSetFlagTrigger : Trigger
{
    private bool main;
    public string ID;
    private string flag;

    public OverlapPairSetFlagTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        main = data.Bool("main");
        ID = data.Attr("id");
        flag = data.Attr("flag");
    }

    public override void Update()
    {
        base.Update();
        if (!main)
            return;

        bool on = false;
        foreach (OverlapPairSetFlagTrigger tri in CollideAll<OverlapPairSetFlagTrigger>())
        {
            if (tri.ID == ID)
            {
                on = true;
                break;
            }
        }

        this.Session().SetFlag(flag, on);
    }
}