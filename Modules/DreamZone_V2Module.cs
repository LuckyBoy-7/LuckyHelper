using System.Reflection;
using LuckyHelper.Entities;
using LuckyHelper.Extensions;
using LuckyHelper.Module;
using LuckyHelper.Utils;
using MonoMod.Cil;

namespace LuckyHelper.Modules;

public class DreamZone_V2Module
{
    public static DreamZone_V2 DreamZone;

    [Load]
    public static void Load()
    {
        On.Celeste.Player.DreamDashUpdate += PlayerOnDreamDashUpdate;
        IL.Celeste.DreamBlock.Setup += DreamBlockOnSetup;
        On.Celeste.Player.Update += PlayerOnUpdate;
        On.Celeste.Player.DreamDashCheck += PlayerOnDreamDashCheck;
    }


    [Unload]
    public static void Unload()
    {
        On.Celeste.Player.DreamDashUpdate -= PlayerOnDreamDashUpdate;
        IL.Celeste.DreamBlock.Setup -= DreamBlockOnSetup;
        On.Celeste.Player.Update -= PlayerOnUpdate;
        On.Celeste.Player.DreamDashCheck -= PlayerOnDreamDashCheck;
    }

    private static bool PlayerOnDreamDashCheck(On.Celeste.Player.orig_DreamDashCheck orig, Player self, Vector2 dir)
    {
        SetDreamZone_V2Collidable(self, true, zone => zone.playerHasDreamDash);
        bool ans = orig(self, dir);
        SetDreamZone_V2Collidable(self, false);
        return ans;
    }

    private static void PlayerOnUpdate(On.Celeste.Player.orig_Update orig, Player self)
    {
        // 获取当前接触的zone
        DreamZone = null;
        foreach (var zone in self.Tracker().GetEntities<DreamZone_V2>())
        {
            bool backup = zone.Collidable;
            zone.Collidable = true;
            DreamZone = self.CollideFirst<DreamZone_V2>();
            zone.Collidable = backup;

            if (DreamZone != null)
                break;
        }

        bool dreamdashing = self.StateMachine.State is Player.StDreamDash;
        if (dreamdashing)
            SetDreamZone_V2Collidable(self, true, zone => zone.playerHasDreamDash);
        orig(self);
        if (dreamdashing)
            SetDreamZone_V2Collidable(self, false);
        else if (self.Speed != Vector2.Zero && self.DashAttacking && DreamZone is { DisableInteraction: false, playerHasDreamDash: true })
        {
            self.StateMachine.State = Player.StDreamDash;
            self.dashAttackTimer = 0f;
            self.gliderBoostTimer = 0f;
        }
    }


    private static void DreamBlockOnSetup(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        if (!cursor.TryGotoNext(ins => ins.MatchLdcR8(0.699999988079071)
            ))
            return;
        cursor.Index += 1;
        cursor.EmitLdarg0();
        cursor.EmitDelegate<Func<double, DreamBlock, double>>((starNumberPerUnit, dreamBlock) =>
        {
            if (dreamBlock is DreamZone_V2 dreamZone)
            {
                return dreamZone.StarNumberPerUnit;
            }

            return starNumberPerUnit;
        });
    }


    private static void SetDreamZone_V2Collidable(Player self, bool on, Func<DreamZone_V2, bool> condition = null)
    {
        foreach (DreamZone_V2 zone in self.Tracker().GetEntities<DreamZone_V2>())
        {
            if (condition == null || condition.Invoke(zone))
            {
                zone.Collidable = on;
            }
        }
    }


    private static int PlayerOnDreamDashUpdate(On.Celeste.Player.orig_DreamDashUpdate orig, Player self)
    {
        int state = orig(self);
        if (DreamZone != null && self.Tracker().GetEntities<Solid>().Any(solid => solid is not DreamZone_V2 && self.CollideCheck(solid)))
        {
            if (DreamZone.StopPlayerOnCollide)
            {
                SetDreamZone_V2Collidable(self, false);
                WigglePlayer(self);
                return Player.StNormal;
            }

            if (DreamZone.KillPlayerOnCollide)
            {
                self.Die(Vector2.Zero);
            }
        }

        // 如果在冲刺过程中果冻关闭了, 那就取消果冻状态
        bool inActiveDreamZone_V2 = false;
        bool tryCancelDreamDash = false;
        foreach (DreamZone_V2 zone in self.Tracker().GetEntities<DreamZone_V2>())
        {
            if (!zone.CollideCheck(self))
                continue;

            if (zone.CancelDreamDashOnNotDreaming)
                tryCancelDreamDash = true;
            if (zone.playerHasDreamDash)
                inActiveDreamZone_V2 = true;
        }

        if (tryCancelDreamDash && !inActiveDreamZone_V2)
            return Player.StNormal;

        return state;
    }

    private static void WigglePlayer(Player self)
    {
        int wiggle = 100;
        for (int i = 1; i <= wiggle; i++)
        {
            int j = i;
            if (i != 0 || j != 0)
            {
                for (int k = 1; k >= -1; k -= 2)
                {
                    for (int l = 1; l >= -1; l -= 2)
                    {
                        Vector2 vector = new Vector2(i * k, j * l);
                        if (!self.CollideCheck<Solid>(self.Position + vector))
                        {
                            self.Position += vector;
                            return;
                        }
                    }
                }
            }
        }
    }
}