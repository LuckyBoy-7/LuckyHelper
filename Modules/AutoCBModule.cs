using System.Reflection;
using LuckyHelper.Module;
using LuckyHelper.Utils;
using Microsoft.Xna.Framework.Input;
using Mono.Cecil.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace LuckyHelper.Modules;

public static class AutoCBModule
{
    private static float Radius(Player self) => Lerp(34, 110, (self.Speed.X + self.Speed.Y * 0.3f) / 1000);
    private static float Degree(Player self) => Lerp(46, 6, (self.Speed.X + self.Speed.Y * 0.3f) / 1000);
    private static float Radians(float degree) => degree / 180 * Single.Pi;

    private static Vector2 DeltaRightRay(Player self) =>
        new(Radius(self) * (float)Math.Cos(Radians(Degree(self))), -Radius(self) * (float)Math.Sin(Radians(Degree(self))));

    private static Vector2 DeltaLeftRay(Player self) =>
        new(-Radius(self) * (float)Math.Cos(Radians(Degree(self))), -Radius(self) * (float)Math.Sin(Radians(Degree(self))));

    private static Grid Grid(Player self) => self.Scene.Tracker.GetEntity<SolidTiles>().Grid;

    // 搞了半天原来只是线被挡住了（
    private static Vector2 BottomLeft(Player self) => new Vector2(self.BottomLeft.X, self.BottomLeft.Y - 1);
    private static Vector2 BottomRight(Player self) => new Vector2(self.BottomRight.X - 1, self.BottomRight.Y - 1);

    private static DynamicData PlayerData;
    private static Vector2 DashDir = Vector2.Zero;
    private static float ControlLastAimTimer = -1;
    private const float ControlLastAimTime = 0.2f;
    private static ILHook dashCoroutineHook;

    // private delegate void OrigPlayerUpdate(On.Celeste.Player.orig_Update orig, Celeste.Player self);
    [Load]
    public static void Load()
    {
        On.Celeste.Player.Update += PlayerOnUpdate;
        On.Celeste.Player.Render += PlayerOnRender;
        var methodInfo = typeof(Player).GetMethod("DashCoroutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget();
        // dashCoroutineHook = new ILHook(methodInfo, ILHookDashCoroutine);
        // Logger.Log(LogLevel.Info, "Test", "456");
    }


    [Unload]
    public static void Unload()
    {
        On.Celeste.Player.Update -= PlayerOnUpdate;
        On.Celeste.Player.Render -= PlayerOnRender;
        // dashCoroutineHook.Dispose();
    }

    private static void ILHookDashCoroutine(ILContext il)
    {
        // 而且好像因为抄celeste tas的load方式现在好像打印会打两次
        var vector2MulFunc = typeof(Vector2).GetMethod("op_Multiply", BindingFlags.Instance | BindingFlags.Public);
        var vector2AddFunc = typeof(Vector2).GetMethod("Add", BindingFlags.Static, new[] { typeof(Vector2), typeof(Vector2) });
        
        ILCursor cur = new(il);
        // 改lastAim
        // Logger.Log(LogLevel.Info, "Test", "123");
        if (cur.TryGotoNext(ins => ins.MatchLdfld<Vector2>("lastAim")))
        {
            // Logger.Log(LogLevel.Info, "Test", "sldkjf");
            // Logger.Log(LogLevel.Info, "Test", cur.Body.ToString());
            cur.Index += 1;
            // cur.EmitDelegate(() => ControlLastAimTimer > 0 ? 0 : 1);
            cur.EmitDelegate(() => -1);
            // cur.Emit(OpCodes.Mul);
            cur.Emit(OpCodes.Call, vector2MulFunc);
            // cur.EmitDelegate(() => ControlLastAimTimer > 0 ? DashDir : Vector2.Zero);
            // // cur.Emit(OpCodes.Call, vector2AddFunc!);
            // cur.Emit(OpCodes.Add);
            // cur.Emit<>(opco  new Vector2(0, 1));
        }
    }

    private static void PlayerOnUpdate(On.Celeste.Player.orig_Update orig, Player self)
    {
        if (!LuckyHelperModule.Settings.EnableAutoCB)
        {
            orig(self);
            return;
        }

        PlayerData ??= DynamicData.For(self);
        if (ControlLastAimTimer > 0)
            ControlLastAimTimer -= Engine.DeltaTime;

        bool flag = false;
        var right = LineOverlapSolidTiles(self, BottomRight(self), BottomRight(self) + DeltaRightRay(self));
        // 如果向右上的射线碰到砖了
        if (right.Item1)
        {
            // 并且砖上没方块，就尝试冲刺
            flag = self.Facing == Facings.Right && !Grid(self).Collide(right.Item2 + new Vector2(0, -8));
            DashDir = new Vector2(1, -1) / (float)Math.Pow(2, 0.5f);
        }

        if (!flag)
        {
            var left = LineOverlapSolidTiles(self, BottomLeft(self), BottomLeft(self) + DeltaLeftRay(self));
            if (left.Item1)
            {
                // 并且砖上没方块，就尝试冲刺
                flag = self.Facing == Facings.Left && !Grid(self).Collide(left.Item2 + new Vector2(0, -8));
                DashDir = new Vector2(-1, -1) / (float)Math.Pow(2, 0.5f);
            }
        }

        if (flag)
            ControlLastAimTimer = ControlLastAimTime;

        // Logger.Log(LogLevel.Info, "Test", Input.Dash.Check.ToString());
        if (flag && !(PlayerData.Get<float>("dashCooldownTimer") > 0.0 || self.Dashes <= 0 || TalkComponent.PlayerOver != null && Input.Talk.Pressed)
                 && (self.LastBooster == null || !self.LastBooster.Ch9HubTransition || !self.LastBooster.BoostingPlayer))
        {
            self.StateMachine.State = self.StartDash();
        }

        orig(self);
    }

    /// <summary>
    /// 判断线段是否和grid相交并且返回对应砖的位置
    /// </summary>
    private static Tuple<bool, Vector2> LineOverlapSolidTiles(Player self, Vector2 from, Vector2 to, int resolution = 10)
    {
        for (int i = 0; i <= resolution; i++)
        {
            if (Grid(self).Collide(Vector2.Lerp(from, to, i * 1f / resolution)))
                return new(true, Vector2.Lerp(from, to, i * 1f / resolution));
        }

        return new(false, Vector2.Zero);
    }


    private static void PlayerOnRender(On.Celeste.Player.orig_Render orig, Player self)
    {
        orig(self);
        if (!LuckyHelperModule.Settings.EnableAutoCB)
            return;
        Draw.Line(BottomLeft(self), BottomLeft(self) + DeltaLeftRay(self), Color.Green);
        Draw.Line(BottomRight(self), BottomRight(self) + DeltaRightRay(self), Color.Green);
    }




    private static float Lerp(float a, float b, float k)
    {
        k = Math.Clamp(k, 0, 1);
        k = 1 - (float)Math.Cos((k * Math.PI) / 2);
        return a + (b - a) * k;
    }
}