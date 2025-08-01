using System.Reflection;
using LuckyHelper.Entities;
using LuckyHelper.Extensions;
using LuckyHelper.Module;
using LuckyHelper.Utils;
using Mono.Cecil;

namespace LuckyHelper.Modules;

public class DreamZoneModule
{
    private static DreamZone dreamZone;
    private static List<DreamZone> playerStartDashInDreamzones = new();

    [Load]
    public static void Load()
    {
        On.Celeste.Player.DashBegin += PlayerOnDashBegin;
        On.Celeste.Player.Update += PlayerOnUpdate;
        IL.Celeste.Player.DreamDashUpdate += PlayerOnDreamDashUpdate;
        On.Celeste.Player.DreamDashUpdate += PlayerOnDreamDashUpdate;
    }

    [Unload]
    public static void Unload()
    {
        On.Celeste.Player.DashBegin -= PlayerOnDashBegin;
        On.Celeste.Player.Update -= PlayerOnUpdate;
        IL.Celeste.Player.DreamDashUpdate -= PlayerOnDreamDashUpdate;
        On.Celeste.Player.DreamDashUpdate -= PlayerOnDreamDashUpdate;
    }

    private static void PlayerOnDashBegin(On.Celeste.Player.orig_DashBegin orig, Player self)
    {
        orig(self);
        SetDreamZoneCollidable(self, true);
        playerStartDashInDreamzones = self.CollideAll<DreamZone>().Cast<DreamZone>().ToList();
        SetDreamZoneCollidable(self, false);
    }

    private static void PlayerOnUpdate(On.Celeste.Player.orig_Update orig, Player self)
    {
        // 获取当前接触的zone
        dreamZone = null;
        foreach (var zone in self.Tracker().GetEntities<DreamZone>())
        {
            bool backup = zone.Collidable;
            zone.Collidable = true;
            dreamZone = self.CollideFirst<DreamZone>();
            zone.Collidable = backup;

            if (dreamZone != null)
                break;
        }

        // 开关zone
        bool on = self.StateMachine.State is Player.StDash or Player.StDreamDash || self.DashAttacking;
        SetDreamZoneCollidable(self, on, zone => !zone.DisableInteraction // 可交互
                                                 // 果冻开启状态, 或者关闭状态但是从外面开始冲
                                                 && (zone.playerHasDreamDash || (!playerStartDashInDreamzones.Contains(zone) && !zone.DisableCollisionOnNotDreaming)));
        // 如果
        orig(self);
        SetDreamZoneCollidable(self, false);
    }


    private static void SetDreamZoneCollidable(Player self, bool on, Func<DreamZone, bool> condition = null)
    {
        foreach (DreamZone zone in self.Tracker().GetEntities<DreamZone>())
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
        if (dreamZone != null && self.Tracker().GetEntities<Solid>().Any(solid => solid is not DreamZone && self.CollideCheck(solid)))
        {
            if (dreamZone.StopPlayerOnCollide)
            {
                SetDreamZoneCollidable(self, false);
                WigglePlayer(self);
                return Player.StNormal;
            }

            if (dreamZone.KillPlayerOnCollide)
            {
                self.Die(Vector2.Zero);
            }
        }

        // 如果在冲刺过程中果冻关闭了, 那就取消果冻状态
        bool inActiveDreamZone = false;
        bool tryCancelDreamDash = false;
        foreach (DreamZone zone in self.Tracker().GetEntities<DreamZone>())
        {
            if (!zone.CollideCheck(self))
                continue;

            if (zone.CancelDreamDashOnNotDreaming)
                tryCancelDreamDash = true;
            if (zone.playerHasDreamDash)
                inActiveDreamZone = true;
        }

        if (tryCancelDreamDash && !inActiveDreamZone)
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

    private static void PlayerOnDreamDashUpdate(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        FieldInfo dreamDashCanEndTimerField = typeof(Player).GetField(
            "dreamDashCanEndTimer", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
        );
        if (!cursor.TryGotoNext(
                ins => ins.MatchLdarg0(),
                ins => ins.MatchLdfld(dreamDashCanEndTimerField),
                ins => ins.MatchLdcR4(0),
                ins => ins.MatchBleUn(out ILLabel il0043)
            ))
            return;

        // if (Input.Jump.Pressed)
        ILLabel outLabel = cursor.DefineLabel();
        FieldInfo inputJump = typeof(Input).GetField("Jump", BindingFlags.Public | BindingFlags.Static);
        MethodInfo getPressed = typeof(VirtualButton).GetMethod("get_Pressed", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        cursor.EmitLdsfld(inputJump);
        cursor.EmitCallvirt(getPressed);
        cursor.EmitBrfalse(outLabel);

        // and touching with dream zone
        cursor.EmitDelegate(() => dreamZone != null
        );
        cursor.EmitBrfalse(outLabel);

        // this.dreamJump = true
        FieldInfo playerDreamJump = typeof(Player).GetField("dreamJump", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        cursor.EmitLdarg0(); // this.
        cursor.EmitLdcI4(1);
        cursor.EmitStfld(playerDreamJump);

        // this.Jump(true, true)
        MethodInfo jumpMethod = typeof(Player).GetMethod("Jump", new[] { typeof(bool), typeof(bool) });
        cursor.EmitLdarg0();
        cursor.EmitLdcI4(1);
        cursor.EmitLdcI4(1);
        cursor.EmitCallvirt(jumpMethod);

        // return 0
        cursor.EmitLdcI4(0);
        cursor.EmitRet();

        cursor.MarkLabel(outLabel);
    }
}