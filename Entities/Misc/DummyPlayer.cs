using Celeste.Mod.Entities;
using LuckyHelper.Extensions;
using MonoMod.Utils;

namespace LuckyHelper.Entities.Misc;

[Tracked]
[CustomEntity("LuckyHelper/DummyPlayer")]
public class DummyPlayer : Player
{
    public const string TriggeredTokenPrefix = "LuckyHelper_Triggered";

    private string triggeredToken;
    private bool sendOriginalPlayerToTrigger;

    // todo: lerp 可能还真得手动钩一下
    public DummyPlayer(EntityData data, Vector2 offset) : base(data.Position + offset, PlayerSpriteMode.Madeline)
    {
        sendOriginalPlayerToTrigger = data.Bool("sendOriginalPlayerToTrigger");
        Visible = false;
        // 必须比 move container 之类的的 container 还晚更新, 不然会导致设置 trigger 和 collide 时 Dummy Player 的状态不一致 
        Depth = -10000001;
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        triggeredToken = TriggeredTokenPrefix + this.GetEntities<DummyPlayer>().IndexOf(this);
    }

    public override void Update()
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