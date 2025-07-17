using System.Reflection;
using LuckyHelper.Extensions;
using LuckyHelper.Module;
using LuckyHelper.Triggers;
using MonoMod.RuntimeDetour;

namespace LuckyHelper.Modules;

public class PlayerMovementControllerModule
{
    // private static List<PlayerMovementHandler> handlers = new();
    private static PlayerJumpForceXHandler jumpForceXHandler = new PlayerJumpForceXHandler();
    private static PlayerJumpForceYHandler jumpForceYHandler = new PlayerJumpForceYHandler();

    private static PlayerWallJumpForceXHandler wallJumpForceXHandler = new PlayerWallJumpForceXHandler();
    private static PlayerWallJumpForceYHandler wallJumpForceYHandler = new PlayerWallJumpForceYHandler();

    private static PlayerJumpKeepSpeedTimeHandler jumpKeepSpeedTimeHandler = new PlayerJumpKeepSpeedTimeHandler();
    private static PlayerWallJumpKeepSpeedTimeHandler wallJumpKeepSpeedTimeHandler = new PlayerWallJumpKeepSpeedTimeHandler();

    private static PlayerLowSpeedAccelerationXHandler lowSpeedAccelerationXHandler = new PlayerLowSpeedAccelerationXHandler();
    private static PlayerHighSpeedAccelerationXHandler highSpeedAccelerationXHandler = new PlayerHighSpeedAccelerationXHandler();
    
    private static PlayerMaxFallSpeedHandler maxFallSpeedHandler = new PlayerMaxFallSpeedHandler();
    private static PlayerMaxFastFallSpeedHandler maxFastFallSpeedHandler = new PlayerMaxFastFallSpeedHandler();
    
    private static PlayerMaxSpeedXMultiplierHandler maxSpeedXMultiplierHandler = new PlayerMaxSpeedXMultiplierHandler();

    private static PlayerMovementController controller;
    private static ILHook wallJumpHook;

    [Load]
    public static void Load()
    {
        // handlers.Add(new PlayerJumpForceHandler());

        On.Celeste.Player.Update += PlayerOnUpdate;
        IL.Celeste.Player.Jump += PlayerOnJump;

        MethodInfo origWallJumpMethod = typeof(Player).GetMethod("orig_WallJump", BindingFlags.Instance | BindingFlags.NonPublic);
        wallJumpHook = new ILHook(origWallJumpMethod, WallJumpHook);

        IL.Celeste.Player.NormalUpdate += PlayerOnNormalUpdate;
    }

    [Unload]
    public static void Unload()
    {
        IL.Celeste.Player.Jump -= PlayerOnJump;
        On.Celeste.Player.Update -= PlayerOnUpdate;
        wallJumpHook.Dispose();
        IL.Celeste.Player.NormalUpdate -= PlayerOnNormalUpdate;
    }

    private static void PlayerOnNormalUpdate(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);

        FieldInfo levelField = typeof(Player).GetField("level", BindingFlags.Instance|BindingFlags.NonPublic);
        FieldInfo inSpaceFiel = typeof(Level).GetField("InSpace", BindingFlags.Instance|BindingFlags.Public);
        if (cursor.TryGotoNext(
                ins => ins.MatchLdarg0(),
                ins => ins.MatchLdfld(levelField),
                ins => ins.MatchLdfld(inSpaceFiel),
                ins => ins.MatchBrfalse(out ILLabel _)
            ))
        {
            cursor.EmitLdloc(6);

            cursor.EmitLdcR4(1f);
            cursor.EmitDelegate<Func<float, float>>(origNumber => maxSpeedXMultiplierHandler.GetHandledData(origNumber, controller));
            cursor.EmitMul();
            cursor.EmitStloc(6);
        }
        
        
        if (cursor.TryGotoNext(ins => ins.MatchLdcR4(400f)
            ))
        {
            cursor.Index += 1;
            cursor.EmitDelegate<Func<float, float>>(origNumber => lowSpeedAccelerationXHandler.GetHandledData(origNumber, controller));
        }

        if (cursor.TryGotoNext(ins => ins.MatchLdcR4(1000f)
            ))
        {
            cursor.Index += 1;
            cursor.EmitDelegate<Func<float, float>>(origNumber => highSpeedAccelerationXHandler.GetHandledData(origNumber, controller));
        }
        
        if (cursor.TryGotoNext(ins => ins.MatchLdcR4(160f)
            ))
        {
            cursor.Index += 1;
            cursor.EmitDelegate<Func<float, float>>(origNumber => maxFallSpeedHandler.GetHandledData(origNumber, controller));
        }
        
        if (cursor.TryGotoNext(ins => ins.MatchLdcR4(240f)
            ))
        {
            cursor.Index += 1;
            cursor.EmitDelegate<Func<float, float>>(origNumber => maxFastFallSpeedHandler.GetHandledData(origNumber, controller));
        }
    }

    private static void PlayerOnUpdate(On.Celeste.Player.orig_Update orig, Player self)
    {
        orig(self);
        controller = self.CollideFirst<PlayerMovementController>();
    }

    private static void WallJumpHook(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        if (cursor.TryGotoNext(ins => ins.MatchLdcR4(0.2f)
            ))
        {
            cursor.Index += 1;
            cursor.EmitDelegate<Func<float, float>>(origNumber => wallJumpKeepSpeedTimeHandler.GetHandledData(origNumber, controller));
        }

        if (cursor.TryGotoNext(ins => ins.MatchLdcR4(130f)
            ))
        {
            cursor.Index += 1;
            cursor.EmitDelegate<Func<float, float>>(origNumber => wallJumpForceXHandler.GetHandledData(origNumber, controller));
        }

        if (cursor.TryGotoNext(ins => ins.MatchLdcR4(-105f)
            ))
        {
            cursor.Index += 1;
            cursor.EmitDelegate<Func<float, float>>(origNumber => wallJumpForceYHandler.GetHandledData(origNumber, controller));
        }
    }

    private static void PlayerOnJump(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        if (cursor.TryGotoNext(ins => ins.MatchLdcR4(0.2f)
            ))
        {
            cursor.Index += 1;
            cursor.EmitDelegate<Func<float, float>>(origNumber => jumpKeepSpeedTimeHandler.GetHandledData(origNumber, controller));
        }

        if (cursor.TryGotoNext(ins => ins.MatchLdcR4(40f)
            ))
        {
            cursor.Index += 1;
            cursor.EmitDelegate<Func<float, float>>(origNumber => jumpForceXHandler.GetHandledData(origNumber, controller));
        }

        if (cursor.TryGotoNext(ins => ins.MatchLdcR4(-105f)
            ))
        {
            cursor.Index += 1;
            cursor.EmitDelegate<Func<float, float>>(origNumber => jumpForceYHandler.GetHandledData(origNumber, controller));
        }
    }
}

public abstract class PlayerMovementHandler
{
    public float GetHandledData(float origNumber, PlayerMovementController controller)
    {
        if (controller != null && controller.ActivationType == ActivationType.Stay)
            return GetStayData(controller);

        var session = LuckyHelperModule.Session;
        var useSetData = session.playerUseSetMovementData;
        if (useSetData)
            return GetSetData(session);
        return origNumber;
    }

    protected abstract float GetSetData(LuckyHelperSession session);
    protected abstract float GetStayData(PlayerMovementController controller);
}

public class PlayerJumpForceXHandler : PlayerMovementHandler
{
    protected override float GetSetData(LuckyHelperSession session) => session.PlayerMovementData.JumpForceX;

    protected override float GetStayData(PlayerMovementController controller) => controller.PlayerMovementData.JumpForceX;
}

public class PlayerJumpForceYHandler : PlayerMovementHandler
{
    protected override float GetSetData(LuckyHelperSession session) => session.PlayerMovementData.JumpForceY;

    protected override float GetStayData(PlayerMovementController controller) => controller.PlayerMovementData.JumpForceY;
}

public class PlayerWallJumpForceXHandler : PlayerMovementHandler
{
    protected override float GetSetData(LuckyHelperSession session) => session.PlayerMovementData.WallJumpForceX;

    protected override float GetStayData(PlayerMovementController controller) => controller.PlayerMovementData.WallJumpForceX;
}

public class PlayerWallJumpForceYHandler : PlayerMovementHandler
{
    protected override float GetSetData(LuckyHelperSession session) => session.PlayerMovementData.WallJumpForceY;

    protected override float GetStayData(PlayerMovementController controller) => controller.PlayerMovementData.WallJumpForceY;
}

public class PlayerJumpKeepSpeedTimeHandler : PlayerMovementHandler
{
    protected override float GetSetData(LuckyHelperSession session) => session.PlayerMovementData.JumpKeepSpeedTime;

    protected override float GetStayData(PlayerMovementController controller) => controller.PlayerMovementData.JumpKeepSpeedTime;
}

public class PlayerWallJumpKeepSpeedTimeHandler : PlayerMovementHandler
{
    protected override float GetSetData(LuckyHelperSession session) => session.PlayerMovementData.WallJumpKeepSpeedTime;

    protected override float GetStayData(PlayerMovementController controller) => controller.PlayerMovementData.WallJumpKeepSpeedTime;
}

public class PlayerLowSpeedAccelerationXHandler : PlayerMovementHandler
{
    protected override float GetSetData(LuckyHelperSession session) => session.PlayerMovementData.LowSpeedAccelerationX;

    protected override float GetStayData(PlayerMovementController controller) => controller.PlayerMovementData.LowSpeedAccelerationX;
}

public class PlayerHighSpeedAccelerationXHandler : PlayerMovementHandler
{
    protected override float GetSetData(LuckyHelperSession session) => session.PlayerMovementData.HighSpeedAccelerationX;

    protected override float GetStayData(PlayerMovementController controller) => controller.PlayerMovementData.HighSpeedAccelerationX;
}

public class PlayerMaxSpeedXMultiplierHandler : PlayerMovementHandler
{
    protected override float GetSetData(LuckyHelperSession session) => session.PlayerMovementData.MaxSpeedXMultiplier;

    protected override float GetStayData(PlayerMovementController controller) => controller.PlayerMovementData.MaxSpeedXMultiplier;
}
public class PlayerMaxFallSpeedHandler : PlayerMovementHandler
{
    protected override float GetSetData(LuckyHelperSession session) => session.PlayerMovementData.MaxFallSpeed;

    protected override float GetStayData(PlayerMovementController controller) => controller.PlayerMovementData.MaxFallSpeed;
}public class PlayerMaxFastFallSpeedHandler : PlayerMovementHandler
{
    protected override float GetSetData(LuckyHelperSession session) => session.PlayerMovementData.MaxFastFallSpeed;

    protected override float GetStayData(PlayerMovementController controller) => controller.PlayerMovementData.MaxFastFallSpeed;
}