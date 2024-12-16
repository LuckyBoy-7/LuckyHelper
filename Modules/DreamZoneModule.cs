using System.Reflection;
using LuckyHelper.Entities;
using LuckyHelper.Extensions;
using LuckyHelper.Module;
using Mono.Cecil;

namespace LuckyHelper.Modules;

public class DreamZoneModule
{
    private static DreamZone dreamZone;
    private static bool isDebug = false;

    [Load]
    public static void Load()
    {
        On.Celeste.Player.Update += PlayerOnUpdate;
        IL.Celeste.Player.DreamDashUpdate += PlayerOnDreamDashUpdate;
        On.Celeste.Player.DreamDashUpdate += PlayerOnDreamDashUpdate;
    }



    [Unload]
    public static void Unload()
    {
        On.Celeste.Player.Update -= PlayerOnUpdate;
        IL.Celeste.Player.DreamDashUpdate -= PlayerOnDreamDashUpdate;
        On.Celeste.Player.DreamDashUpdate -= PlayerOnDreamDashUpdate;
    }


    private static void PlayerOnUpdate(On.Celeste.Player.orig_Update orig, Player self)
    {
        // 获取当前接触的zone
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
        SetDreamZoneCollidable(self, on);
        orig(self);
    }

    private static void SetDreamZoneCollidable(Player self, bool on)
    {
        foreach (var zone in self.Tracker().GetEntities<DreamZone>())
        {
            zone.Collidable = on;
        }
    }


    private static int PlayerOnDreamDashUpdate(On.Celeste.Player.orig_DreamDashUpdate orig, Player self)
    {
        if (dreamZone != null && self.Tracker().GetEntities<Solid>().Any(solid => solid is not DreamZone && self.CollideCheck(solid)))
        {
            if (dreamZone.stopPlayerOnCollide)
            {
                SetDreamZoneCollidable(self, false);
                WigglePlayer(self);
                return Player.StNormal;
            }

            if (dreamZone.killPlayerOnCollide)
            {
                self.Die(Vector2.Zero);
            }
        }

        return orig(self);
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