using Celeste.Mod.Entities;
using Lucky.Kits.Collections;
using LuckyHelper.Extensions;
using LuckyHelper.Modules;

namespace LuckyHelper.Triggers;

[CustomEntity("LuckyHelper/OverlapPairSetFlagTrigger")]
[Tracked]
public class OverlapPairSetFlagTrigger : Trigger
{
    private bool main;
    public string ID;
    private string flag;
    private Vector2 targetPos;

    public OverlapPairSetFlagTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        main = data.Bool("main");
        ID = data.Attr("triggerID");
        flag = data.Attr("flag");
        targetPos = Position;
        if (data.Nodes.Length != 0)
        {
            targetPos += data.Nodes[0] + offset - Center;
        }
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        Position = targetPos;
    }


    public override void Update()
    {
        base.Update();
        if (!main)
            return;

        bool on = false;
        foreach (OverlapPairSetFlagTrigger tri in this.GetEntities<OverlapPairSetFlagTrigger>()
                     .Where(entity => ((OverlapPairSetFlagTrigger)entity).ID == ID))
        {
            if (tri != null && tri != this && CollideCheck(tri))
            {
                on = true;
                break;
            }
        }

        this.Session().SetFlag(flag, on);
    }
}