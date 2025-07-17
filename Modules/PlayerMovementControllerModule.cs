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
    private static PlayerJumpKeepSpeedTimeHandler wallJumpKeepSpeedTimeHandler = new PlayerJumpKeepSpeedTimeHandler();

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

    [Unload]
    public static void Unload()
    {
        IL.Celeste.Player.Jump -= PlayerOnJump;
        On.Celeste.Player.Update -= PlayerOnUpdate;
        wallJumpHook.Dispose();
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