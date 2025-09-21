using Celeste.Mod.Entities;
using LuckyHelper.Module;

namespace LuckyHelper.Triggers;

[CustomEntity("LuckyHelper/PlayerInvincibleController")]
[Tracked]
public class PlayerInvincibleController : Trigger
{
    public bool InvincibleOnStartDashInSpikes;

    public PlayerInvincibleController(EntityData data, Vector2 offset) : base(data, offset)
    {
        InvincibleOnStartDashInSpikes = data.Bool("invincibleOnStartDashInSpikes");
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        LuckyHelperModule.Session.InvincibleOnStartDashInSpikes = InvincibleOnStartDashInSpikes;
    }

    [Load]
    public static void Load()
    {
        On.Celeste.PlayerCollider.Check += PlayerColliderOnCheck;
    }


    [Unload]
    public static void Unload()
    {
        On.Celeste.PlayerCollider.Check -= PlayerColliderOnCheck;
    }

    private static bool PlayerColliderOnCheck(On.Celeste.PlayerCollider.orig_Check orig, Celeste.PlayerCollider self, Player player)
    {
        if (LuckyHelperModule.Session.InvincibleOnStartDashInSpikes)
            if (player.StartedDashing && self.Entity.GetType().Name.EndsWith("Spikes"))
                return false;

        return orig(self, player);
    }
}