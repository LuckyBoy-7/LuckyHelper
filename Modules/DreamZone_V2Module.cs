using System.Reflection;
using LuckyHelper.Entities;
using LuckyHelper.Extensions;
using LuckyHelper.Module;
using LuckyHelper.Utils;
using MonoMod.Cil;

namespace LuckyHelper.Modules;

public class DreamZone_V2Module
{
    public static DreamZone_V2 CurrentOverlappingDreamZone;
    public static DreamBlock LastPlayerTravelledDreamBlock;

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
        HashSet<DreamZone_V2> outsideDreamZone = new();
        // 获取当前接触的zone
        CurrentOverlappingDreamZone = null;
        foreach (DreamZone_V2 zone in self.Tracker().GetEntities<DreamZone_V2>())
        {
            if (self.SafeCollideCheck(zone))
            {
                CurrentOverlappingDreamZone = zone;
            }
            else
                outsideDreamZone.Add(zone);
        }


        bool dashing = self.StateMachine.State is Player.StDash || self.DashAttacking;
        bool dreamdashing = self.StateMachine.State is Player.StDreamDash;
        if (dashing || dreamdashing)
            // 适配老特性, 即冲刺时禁用状态下的果冻有碰撞
            SetDreamZone_V2Collidable(self, true, zone =>
            {
                if (!zone.UseOldFeature)
                    return false;
                if (zone.DisableInteraction)
                    return false;
                // 说明是在禁用果冻的内部冲刺的
                if (!outsideDreamZone.Contains(zone) && !zone.playerHasDreamDash)
                    return false;
                return true;
            });

        if (dreamdashing)
            SetDreamZone_V2Collidable(self, true, zone => !zone.UseOldFeature && zone.playerHasDreamDash && !zone.DisableInteraction);
        orig(self);
        SetDreamZone_V2Collidable(self, false);
        if (!dreamdashing && self.Speed != Vector2.Zero && self.DashAttacking && CurrentOverlappingDreamZone is { DisableInteraction: false, playerHasDreamDash: true })
        {
            self.StateMachine.State = Player.StDreamDash;
            self.dashAttackTimer = 0f;
            self.gliderBoostTimer = 0f;
            self.dreamBlock = CurrentOverlappingDreamZone;
        }
    }


    private static void DreamBlockOnSetup(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        if (cursor.TryGotoNext(ins => ins.MatchLdcR8(0.699999988079071)
            ))
        {
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

        if (cursor.TryGotoNext(
                ins => ins.MatchLdarg0(),
                ins => ins.MatchLdfld(typeof(DreamBlock).GetField("particles")),
                ins => ins.MatchLdloc0(),
                ins => ins.MatchLdelema(out _),
                ins => ins.MatchLdfld(out _),
                ins => ins.MatchStloc1(),
                ins => ins.MatchLdloc1()
            ))
        {
            cursor.Index += 2;
            ILLabel setParticleStart = cursor.DefineLabel();
            ILLabel setParticleEnd = cursor.DefineLabel();
            cursor.EmitLdarg0();
            cursor.EmitDelegate<Func<DreamBlock, bool>>((dreamBlock) =>
            {
                if (dreamBlock is DreamZone_V2)
                {
                    return true;
                }

                return false;
            });

            cursor.EmitBrfalse(setParticleStart);
            // custom set
            cursor.EmitLdloc0();
            cursor.EmitDelegate<Action<int, DreamBlock>>((index, dreamBlock) =>
            {
                if (dreamBlock is DreamZone_V2 dreamZone)
                {
                    switch (dreamZone.particles[index].Layer)
                    {
                        case 0:
                            dreamZone.particles[index].Color = dreamZone.GetRandomColorByString(dreamZone.BigStarColors);
                            break;
                        case 1:
                            dreamZone.particles[index].Color = dreamZone.GetRandomColorByString(dreamZone.MediumStarColors);
                            break;
                        case 2:
                            dreamZone.particles[index].Color = dreamZone.GetRandomColorByString(dreamZone.SmallStarColors);
                            break;
                    }
                }
            });
            cursor.EmitBr(setParticleEnd);
            cursor.MarkLabel(setParticleStart);


            if (cursor.TryGotoNext(
                    ins => ins.MatchLdloc0(),
                    ins => ins.MatchLdcI4(1)
                ))
            {
                cursor.MarkLabel(setParticleEnd);
            }
        }
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
        if (CurrentOverlappingDreamZone != null && self.Tracker().GetEntities<Solid>().Any(solid => solid is not DreamBlock && self.CollideCheck(solid)))
        {
            if (CurrentOverlappingDreamZone.StopPlayerOnCollide)
            {
                SetDreamZone_V2Collidable(self, false);
                WigglePlayer(self);
                return Player.StNormal;
            }

            if (CurrentOverlappingDreamZone.KillPlayerOnCollide)
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