using System.Reflection;
using LuckyHelper.Entities;
using LuckyHelper.Module;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace LuckyHelper.Modules;

public class CustomWaterModule
{
    private static CustomWater customWater;
    private static bool isDebug = true;

    [Load]
    public static void Load()
    {
        // water surface jump
        On.Celeste.Player.Update += PlayerOnUpdate;
        IL.Celeste.Player.NormalUpdate += PlayerOnNormalUpdate;
        IL.Celeste.Player.SwimUpdate += PlayerOnSwimUpdate;
    }

    private static void PlayerOnUpdate(On.Celeste.Player.orig_Update orig, Player self)
    {
        customWater = self.CollideFirst<CustomWater>();
        orig(self);
    }


    [Unload]
    public static void Unload()
    {
        On.Celeste.Player.Update -= PlayerOnUpdate;
        IL.Celeste.Player.NormalUpdate -= PlayerOnNormalUpdate;
        IL.Celeste.Player.SwimUpdate -= PlayerOnSwimUpdate;
    }

    private static void HookSwimRise(ILCursor cursor)
    {
        cursor.Index = 0;
        // swim rise
        if (!cursor.TryGotoNext(
                ins => ins.MatchLdcR4(-60),
                ins => ins.MatchLdcR4(600)
            ))
            return;

        if (isDebug)
            Logger.Log(LogLevel.Warn, "Test", "SwimRise OK");
        cursor.Index += 1;
        cursor.EmitDelegate(
            () =>
            {
                if (customWater == null || !customWater.DisableSwimRise)
                    return 1;
                return 0;
            }
        );
        cursor.EmitMul();
    }

    private static void HookMaxSpeed(ILCursor cursor)
    {
        // max speed x
        cursor.Index = 0;
        Logger.Log(LogLevel.Warn, "Test", "123 OK");
        if (!cursor.TryGotoNext(
                //todo: 我不知道为什么突然用数字找不管用了(
                // ins => ins.MatchLdcR4(out float eighty),
                // ins => ins.MatchLdcR4(80f),
                ins => ins.MatchLdcR4(80f) && ins.Next.MatchStloc3()
                // ins => ins.MatchStloc3()
                // ins => ins.MatchStloc(3)
            ))
            return;

        if (isDebug)
            Logger.Log(LogLevel.Warn, "Test", "MaxSpeed OK");

        cursor.EmitLdloc(2);
        cursor.EmitDelegate(
            () =>
            {
                if (customWater == null)
                    return 1;
                return customWater.MaxSpeedMultiplierX;
            }
        );
        cursor.EmitMul();
        cursor.EmitStloc(2);
        // // max speed y
        // cursor.Index += 2;
        // cursor.EmitDelegate(
        //     () =>
        //     {
        //         if (customWater == null)
        //             return 1;
        //         return customWater.MaxSpeedMultiplierY;
        //     }
        // );
        // cursor.EmitMul();
    }

    private static void HookAcceleration(ILCursor cursor)
    {
        cursor.Index = 0;
        // friction multiplier
        if (!cursor.TryGotoNext(
                ins => ins.MatchLdcR4(400)
            ))
            return;
        if (isDebug)
            Logger.Log(LogLevel.Warn, "Test", "Acceleration OK");
        cursor.Index += 1;
        cursor.EmitDelegate(
            () =>
            {
                if (customWater == null)
                    return 1;
                return customWater.AccelerationMultiplierX;
            }
        );
        cursor.EmitMul();

        cursor.TryGotoNext(
            ins => ins.MatchLdcR4(600)
        );
        cursor.Index += 1;
        cursor.EmitDelegate(
            () =>
            {
                if (customWater == null)
                    return 1;
                return customWater.AccelerationMultiplierX;
            }
        );
        cursor.EmitMul();
        cursor.TryGotoNext(
            ins => ins.MatchLdcR4(400)
        );
        cursor.Index += 1;
        cursor.EmitDelegate(
            () =>
            {
                if (customWater == null)
                    return 1;
                return customWater.AccelerationMultiplierY;
            }
        );
        cursor.EmitMul();

        cursor.TryGotoNext(
            ins => ins.MatchLdcR4(600)
        );
        cursor.Index += 1;
        cursor.EmitDelegate(
            () =>
            {
                if (customWater == null)
                    return 1;
                return customWater.AccelerationMultiplierY;
            }
        );
        cursor.EmitMul();
    }

    private static void HookSwimJump(ILCursor cursor)
    {
        FieldInfo inputJump = typeof(Input).GetField("Jump", BindingFlags.Public | BindingFlags.Static);
        MethodInfo getPressed = typeof(VirtualButton).GetMethod("get_Pressed", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        ILLabel outLabel = null;
        cursor.Index = 0;
        if (!cursor.TryGotoNext(
                ins => ins.MatchLdsfld(inputJump),
                ins => ins.MatchCallvirt(getPressed),
                ins => ins.MatchBrfalse(out outLabel)
            ))
            return;
        cursor.Index += 3;
        if (isDebug)
            Logger.Log(LogLevel.Warn, "Test", "SwimJumpCheck OK");
        // player can jump
        ILLabel jumpLabel = cursor.DefineLabel();
        cursor.EmitDelegate(
            () =>
            {
                if (customWater == null || !customWater.PlayerCanJump)
                    return 1;
                return 0;
            }
        );
        cursor.EmitBrfalse(jumpLabel);

        cursor.EmitDelegate(
            () =>
            {
                if (customWater == null || !customWater.DisableSurfaceJump)
                    return 1;
                return 0;
            }
        );
        cursor.EmitBrfalse(outLabel);

        cursor.Index += 3;
        cursor.MarkLabel(jumpLabel);
    }

    private static void HookLoseControl(ILCursor cursor)
    {
        // lose control
        cursor.Index = 0;
        if (!cursor.TryGotoNext(
                ins => ins.MatchLdsfld(out FieldReference f),
                ins => ins.MatchCallvirt(out MethodReference m),
                ins => ins.MatchStloc1()
            ))
            return;
        if (isDebug)
            Logger.Log(LogLevel.Warn, "Test", "LoseControl OK");
        cursor.Index += 2;
        // MethodInfo zero = typeof(Vector2).GetMethod("get_Zero");
        MethodInfo mul = typeof(Vector2).GetMethod("op_Multiply", new[] { typeof(Vector2), typeof(float) });
        // cursor.EmitCall(zero);
        cursor.EmitDelegate(
            () =>
            {
                if (customWater == null || !customWater.PlayerLoseControl)
                    return 1f; // todo: 这里一定要写成浮点类而不是int
                return 0f;
            }
        );
        cursor.EmitCall(mul);
    }

    private static void HookGravity(ILCursor cursor)
    {
        // gravity
        cursor.Index = 0;
        FieldInfo moveXField = typeof(Player).GetField("moveX", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        FieldInfo speedField = typeof(Player).GetField("Speed", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        FieldInfo yField = typeof(Vector2).GetField("Y", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        MethodInfo deltaTime = typeof(Engine).GetMethod("get_DeltaTime", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        MethodInfo approach = typeof(Calc).GetMethod(
            "Approach", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, new[] { typeof(float), typeof(float), typeof(float) }
        );
        if (!cursor.TryGotoNext(
                ins => ins.MatchLdloc0(),
                ins => ins.MatchBrtrue(out ILLabel label),
                ins => ins.MatchLdarg0(),
                ins => ins.MatchLdfld(moveXField),
                ins => ins.MatchBrfalse(out ILLabel label)
            ))
            return;
        if (isDebug)
            Logger.Log(LogLevel.Warn, "Test", "SwimGravity OK");
        // this.Speed
        cursor.EmitLdarg0();
        cursor.EmitLdflda(speedField);
        // this.Speed.Y
        cursor.EmitLdarg0();
        cursor.EmitLdflda(speedField);
        cursor.EmitLdfld(yField);
        // num2 * Vector.UnitY
        cursor.EmitLdloc3();
        cursor.EmitLdcI4(1);
        cursor.EmitMul();
        // 600 * Engine.DeltaTime
        cursor.EmitLdcR4(600f);
        cursor.EmitCall(deltaTime);
        cursor.EmitMul();
        cursor.EmitDelegate(
            () => customWater == null ? 0 : customWater.PlayerGravity
        );
        cursor.EmitMul();
        // Approach
        cursor.EmitCall(approach);
        // =
        cursor.EmitStfld(yField);
    }

    private static void PlayerOnSwimUpdate(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        HookSwimRise(cursor);
        HookMaxSpeed(cursor);
        // HookAcceleration(cursor);
        // HookSwimJump(cursor);
        // HookLoseControl(cursor);
        // HookGravity(cursor);
    }

    private static void PlayerOnNormalUpdate(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);

        // surface jump
        ILLabel outLabel = null;
        if (!cursor.TryGotoNext(
                ins => ins.MatchLdcI4(0),
                ins => ins.MatchRet(),
                ins => ins.MatchLdloc2(),
                ins => ins.MatchRet()
            ))
            return;
        outLabel = cursor.MarkLabel();

        MethodInfo jumpMethod = typeof(Player).GetMethod("Jump", new[] { typeof(bool), typeof(bool) });
        if (!cursor.TryGotoPrev(
                ins => ins.MatchLdarg0(),
                ins => ins.MatchLdcI4(1),
                ins => ins.MatchLdcI4(1),
                ins => ins.MatchCallvirt(jumpMethod),
                ins => ins.MatchLdloc(13)
            ))
            return;
        ILLabel jumpLabel = cursor.DefineLabel();
        
        if (isDebug)
            Logger.Log(LogLevel.Warn, "test", "WaterSurfaceJump OK");
        cursor.EmitLdloc(13);
        cursor.EmitIsinst(typeof(CustomWater));
        // 如果是普通水就直接跳
        cursor.EmitBrfalse(jumpLabel);

        cursor.EmitLdloc(13);
        cursor.EmitCastclass(typeof(CustomWater));
        // 看看removeSurfaceJump的值, 如果为true, 就直接跳出, 不执行跳跃
        var getRemoveSurfaceJumpFiled = typeof(CustomWater).GetField("DisableSurfaceJump");
        cursor.EmitLdfld(getRemoveSurfaceJumpFiled);
        cursor.EmitBrtrue(outLabel);

        cursor.Index = 0;
        cursor.TryGotoNext(
            ins => ins.MatchLdarg0(),
            ins => ins.MatchLdcI4(1),
            ins => ins.MatchLdcI4(1),
            ins => ins.MatchCallvirt(jumpMethod),
            ins => ins.MatchLdloc(13)
        );
        cursor.MarkLabel(jumpLabel);
    }
}