using Celeste.Mod.Entities;
using LuckyHelper.Entities;

namespace LuckyHelper.Triggers;

[CustomEntity("LuckyHelper/KillGhostTrigger")]
public class KillGhostTrigger : Trigger
{
    private bool killPlayerToo;

    public KillGhostTrigger(EntityData data, Vector2 offset)
        : base(data, offset)
    {
        killPlayerToo = data.Bool("killPlayerToo");
    }

    public override void Update()
    {
        base.Update();
        var ghostTranpose = CollideFirst<GhostTranspose>();
        if (ghostTranpose != null)
        {
            ghostTranpose.Kill();
            if (killPlayerToo)
                Scene.Tracker.GetEntity<Player>()?.Die(Vector2.Zero);
        }
    }
}