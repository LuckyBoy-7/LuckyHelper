using System.Reflection;
using System.Runtime.CompilerServices;
using Celeste.Mod.Entities;
using LuckyHelper.Extensions;
using LuckyHelper.Module;
using LuckyHelper.Utils;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace LuckyHelper.Triggers;

[CustomEntity("LuckyHelper/CameraUpdateHelper")]
[Tracked]
public class CameraUpdateHelper : Trigger
{
    public string DisablePlayerCameraUpdateFlag;
    public string DisableCameraUpdateOnTeleportFlag;

    public CameraUpdateHelper(EntityData data, Vector2 offset) : base(data, offset)
    {
        DisablePlayerCameraUpdateFlag = data.Attr("disablePlayerCameraUpdateFlag");
        DisableCameraUpdateOnTeleportFlag = data.Attr("disableCameraUpdateOnTeleportFlag");
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        var session = LuckyHelperModule.Session;
        session.DisablePlayerCameraUpdateFlag = DisablePlayerCameraUpdateFlag;
        session.DisableCameraUpdateOnTeleportFlag = DisableCameraUpdateOnTeleportFlag;
    }

    public static ILHook playerOrigUpdateHook;

    [Load]
    public static void Load()
    {
        playerOrigUpdateHook = new ILHook(typeof(Player).GetMethod("orig_Update", BindingFlags.Instance | BindingFlags.Public), OnPlayerOrigUpdate);
        IL.Celeste.Level.TeleportTo += LevelOnTeleportTo;
    }

    private static void LevelOnTeleportTo(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        ILLabel outLabel = null;
        if (cursor.TryGotoNext(
                ins => ins.MatchLdarg0(),
                ins => ins.MatchLdfld(out _),
                ins => ins.MatchLdarg1(),
                ins => ins.MatchCallvirt(out _),
                ins => ins.MatchCallvirt(out _)
            ))

        {
            // 为了让传送的时候不改变 camera 的位置
            cursor.Index += 4;
            cursor.EmitLdarg0();
            cursor.EmitDelegate<Func<Vector2, Level, Vector2>>((newCameraPos, level) =>
            {
                if (level.Session.GetFlag(LuckyHelperModule.Session.DisableCameraUpdateOnTeleportFlag))
                    return level.Camera.Position;
                return newCameraPos;
            });
        }
    }

    private static void OnPlayerOrigUpdate(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        ILLabel outLabel = null;
        if (cursor.TryGotoNext(
                ins => ins.MatchLdarg0(),
                ins => ins.MatchCallvirt(out _),
                ins => ins.MatchBrtrue(out _),
                ins => ins.MatchLdarg0(),
                ins => ins.MatchLdfld(out _),
                ins => ins.MatchBrfalse(out outLabel)
            ))
        {
            // 为了让 player 传送的时候 camera 不乱动
            cursor.EmitLdarg0();
            cursor.EmitDelegate<Func<Player, bool>>((player) => player.Session().GetFlag(LuckyHelperModule.Session.DisablePlayerCameraUpdateFlag));
            cursor.EmitBrtrue(outLabel);
        }
    }


    [Unload]
    public static void Unload()
    {
        playerOrigUpdateHook?.Dispose();
        playerOrigUpdateHook = null;
        IL.Celeste.Level.TeleportTo -= LevelOnTeleportTo;
    }
}