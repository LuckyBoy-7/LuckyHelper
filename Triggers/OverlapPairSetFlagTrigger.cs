using Celeste.Mod.Entities;
using Lucky.Kits.Collections;
using LuckyHelper.Extensions;

namespace LuckyHelper.Triggers;

[CustomEntity("LuckyHelper/OverlapPairSetFlagTrigger")]
[Tracked]
public class OverlapPairSetFlagTrigger : Trigger
{
    private bool main;
    public string ID;
    private string flag;
    public static DefaultDict<string, HashSet<OverlapPairSetFlagTrigger>> IdToTriggerSet = new(() => new());

    public OverlapPairSetFlagTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        main = data.Bool("main");
        ID = data.Attr("triggerID");
        flag = data.Attr("flag");
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        IdToTriggerSet[ID].Add(this);
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);
        IdToTriggerSet[ID].Remove(this);
    }

    public override void Update()
    {
        base.Update();
        if (!main)
            return;

        bool on = false;
        foreach (OverlapPairSetFlagTrigger tri in IdToTriggerSet[ID])
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