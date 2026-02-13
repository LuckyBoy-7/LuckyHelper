using Celeste.Mod.Entities;
using LuckyHelper.Extensions;
using LuckyHelper.Module;
using LuckyHelper.Utils;
using MonoMod.Cil;
using MonoMod.Utils;

namespace LuckyHelper.Entities.Misc;

[Tracked]
[CustomEntity("LuckyHelper/DummyPlayer")]
public class DummyPlayer : Player
{
    public const string TriggeredTokenPrefix = "LuckyHelper_Triggered";

    private string triggeredToken;
    private bool sendOriginalPlayerToTrigger;
    private string triggerDashFlag;
    private bool affectedByWind;

    private Vector2 prePosition;

    // todo: lerp 可能还真得手动钩一下
    public DummyPlayer(EntityData data, Vector2 offset) : base(data.Position + offset, PlayerSpriteMode.Madeline)
    {
        sendOriginalPlayerToTrigger = data.Bool("sendOriginalPlayerToTrigger");
        triggerDashFlag = data.Attr("triggerDashFlag", "LuckyHelper_TriggerDashFlag");
        affectedByWind = data.FitBool(true, "canAffectedByWind", "affectedByWind");
        Visible = false;
        // 必须比 move container 之类的的 container 还晚更新, 不然会导致设置 trigger 和 collide 时 Dummy Player 的状态不一致 
        Depth = -10000001;

        if (!affectedByWind)
            Components.RemoveAll<WindMover>();
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        triggeredToken = TriggeredTokenPrefix + this.GetEntities<DummyPlayer>().IndexOf(this);
        // init
        prePosition = Position;
    }

    public override void Update()
    {
        UpdateTrigger();
        UpdateDash();
        prePosition = Position;
    }

    private void UpdateDash()
    {
        Session session = this.Session();
        if (!session.GetFlag(triggerDashFlag))
            return;
        session.SetFlag(triggerDashFlag, false);

        // get 8-directions dash dir
        var dashDir = new Vector2(1, 0);
        Vector2 speed = Position - prePosition;
        if (speed != Vector2.Zero)
        {
            speed.Normalize();
            speed = CorrectDashPrecision(speed);
            speed.Sign();
            dashDir = speed;
        }

        foreach (DashListener dashListener in Scene.Tracker.GetComponents<DashListener>())
        {
            dashListener.OnDash?.Invoke(dashDir);
        }
    }

    private void UpdateTrigger()
    {
        Player player = sendOriginalPlayerToTrigger ? Scene.Tracker.GetEntity<Player>() : this;

        if (player == null)
            return;

        foreach (Trigger trigger in Scene.Tracker.GetEntities<Trigger>())
        {
            DynamicData dd = DynamicData.For(trigger);
            if (!dd.Data.ContainsKey(triggeredToken))
                dd.Set(triggeredToken, false);

            bool triggered = dd.Get<bool>(triggeredToken);
            if (CollideCheck(trigger))
            {
                if (!triggered)
                {
                    dd.Set(triggeredToken, true);
                    trigger.OnEnter(player);
                }

                trigger.OnStay(player);
            }
            else if (triggered)
            {
                dd.Set(triggeredToken, false);
                trigger.OnLeave(player);
            }
        }
    }
}