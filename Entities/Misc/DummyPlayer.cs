using System.Collections;
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
    private string triggerRidingFlag;
    private string triggerCollisionFlag;
    private float affectRadius;
    private bool affectedByWind;
    private WhiteBlacklistChecker whiteBlacklistChecker;

    private Vector2 prePosition;
    private bool forceRiding;

    // todo: lerp 可能还真得手动钩一下
    public DummyPlayer(EntityData data, Vector2 offset) : base(data.Position + offset, PlayerSpriteMode.Madeline)
    {
        sendOriginalPlayerToTrigger = data.Bool("sendOriginalPlayerToTrigger");
        triggerDashFlag = data.Attr("triggerDashFlag", "LuckyHelper_TriggerDashFlag");
        triggerRidingFlag = data.Attr("triggerRidingFlag", "LuckyHelper_TriggerRidingFlag");
        triggerCollisionFlag = data.Attr("triggerCollisionFlag", "LuckyHelper_TriggerCollisionFlag");
        affectRadius = data.Float("affectRadius", 114514);
        affectedByWind = data.FitBool(true, "canAffectedByWind", "affectedByWind");

        whiteBlacklistChecker = new WhiteBlacklistChecker(data);
        Visible = false;
        // 必须比 move container 之类的的 container 还晚更新, 不然会导致设置 trigger 和 collide 时 Dummy Player 的状态不一致 
        Depth = -10000001;

        if (!affectedByWind)
            Components.RemoveAll<WindMover>();

        // 我们要去触发 zipmover 那些, 最好不要被推动
        AllowPushing = false;
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
        ResetStates();
        UpdateTrigger();
        UpdatePlayerCollider();
        UpdateDash();
        prePosition = Position;
    }

    private void ResetStates()
    {
        Dashes = -114;
        Stamina = -514;
        forceRiding = false;
    }

    public bool AffectDistCheck(Entity otherEntity)
    {
        return Vector2.Distance(otherEntity.Position, Position) <= affectRadius;
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
            if (AffectDistCheck(dashListener.Entity) && whiteBlacklistChecker.Check(dashListener.Entity))
                dashListener.OnDash?.Invoke(dashDir);
        }
    }

    private void UpdateTrigger()
    {
        Player player = GetPlayer();
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

    private Player GetPlayer()
    {
        return sendOriginalPlayerToTrigger ? Scene.Tracker.GetEntity<Player>() : this;
    }

    private void UpdatePlayerCollider()
    {
        Player player = GetPlayer();
        if (player == null)
            return;

        Session session = this.Session();
        if (session.GetFlag(triggerCollisionFlag))
        {
            session.SetFlag(triggerCollisionFlag, false);
            foreach (PlayerCollider playerCollider in Scene.Tracker.GetComponents<PlayerCollider>())
            {
                if (playerCollider.Entity.Collidable && AffectDistCheck(playerCollider.Entity) && whiteBlacklistChecker.Check(playerCollider.Entity))
                    playerCollider.OnCollide?.Invoke(player);
            }
        }
        else
        {
            foreach (PlayerCollider playerCollider in Scene.Tracker.GetComponents<PlayerCollider>())
            {
                if (AffectDistCheck(playerCollider.Entity) && whiteBlacklistChecker.Check(playerCollider.Entity))
                {
                    playerCollider.Check(player);
                }
            }
        }
    }

    public override bool IsRiding(Solid solid)
    {
        if (forceRiding)
            return true;
        Session session = this.Session();
        if (session.GetFlag(triggerRidingFlag))
        {
            session.SetFlag(triggerRidingFlag, false);
            forceRiding = true;
            return true;
        }

        return base.IsRiding(solid);
    }

    [Load]
    public static void Load()
    {
        On.Celeste.Solid.GetPlayerRider += SolidOnGetPlayerRider;
        On.Celeste.Player.RefillDash += PlayerOnRefillDash;
        On.Celeste.Player.RefillStamina += PlayerOnRefillStamina;
        On.Celeste.Player.UseRefill += PlayerOnUseRefill;
        On.Celeste.Player.Die += PlayerOnDie;
    }

    [Unload]
    public static void Unload()

    {
        On.Celeste.Solid.GetPlayerRider -= SolidOnGetPlayerRider;
        On.Celeste.Player.RefillDash -= PlayerOnRefillDash;
        On.Celeste.Player.RefillStamina -= PlayerOnRefillStamina;
        On.Celeste.Player.UseRefill -= PlayerOnUseRefill;
        On.Celeste.Player.Die -= PlayerOnDie;
    }

    private static PlayerDeadBody PlayerOnDie(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats)
    {
        if (self is DummyPlayer)
            return null;
        return orig(self, direction, evenIfInvincible, registerDeathInStats);
    }

    private static bool PlayerOnUseRefill(On.Celeste.Player.orig_UseRefill orig, Player self, bool twoDashes)
    {
        if (self is DummyPlayer)
            return true;
        return orig(self, twoDashes);
    }

    private static void PlayerOnRefillStamina(On.Celeste.Player.orig_RefillStamina orig, Player self)
    {
        if (self is not DummyPlayer)
            orig(self);
    }

    private static bool PlayerOnRefillDash(On.Celeste.Player.orig_RefillDash orig, Player self)
    {
        if (self is DummyPlayer)
            return true;
        return orig(self);
    }

    private static Player SolidOnGetPlayerRider(On.Celeste.Solid.orig_GetPlayerRider orig, Solid self)
    {
        Player origRider = orig(self);
        if (origRider != null)
            return origRider;

        foreach (DummyPlayer dummyPlayer in self.Tracker().GetEntities<DummyPlayer>())
        {
            if (!dummyPlayer.whiteBlacklistChecker.Check(self))
                continue;
            if (dummyPlayer.IsRiding(self) && dummyPlayer.AffectDistCheck(self))
            {
                return dummyPlayer;
            }
        }

        return null;
    }
}