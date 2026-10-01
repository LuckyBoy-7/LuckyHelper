using System.Reflection;
using Celeste.Mod.Entities;
using Lucky.Kits.Collections;
using Lucky.Kits.Extensions;
using LuckyHelper.Extensions;
using LuckyHelper.Module;
using LuckyHelper.Modules;
using LuckyHelper.Triggers;
using LuckyHelper.Utils;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace LuckyHelper.Entities.Misc;

[CustomEntity("LuckyHelper/SetFlagOnSwitchGateActivated")]
[Tracked]
public class SetFlagOnSwitchGateActivated : Entity
{
    private string flag;

    public SetFlagOnSwitchGateActivated(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        flag = data.Attr("flag");
    }

    private static ILHook sequenceHook;

    [Load]
    public static void Load()
    {
        var methodInfo = typeof(SwitchGate).GetMethod("Sequence", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget();
        sequenceHook = new ILHook(methodInfo, ILHookSwitchGateSequenceCoroutine);

        On.Celeste.SwitchGate.Awake += SwitchGateOnAwake;
    }

    [Unload]
    public static void Unload()
    {
        sequenceHook?.Dispose();
        sequenceHook = null;

        On.Celeste.SwitchGate.Awake -= SwitchGateOnAwake;
    }

    private static void TrySetFlagThroughEntity(Entity entity)
    {
        Level level = entity.SceneAs<Level>();
        if (level.Tracker.GetEntity<SetFlagOnSwitchGateActivated>() is { } flagEntity)
        {
            level.Session.SetFlag(flagEntity.flag);
        }
    }

    private static void SwitchGateOnAwake(On.Celeste.SwitchGate.orig_Awake orig, SwitchGate self, Scene scene)
    {
        orig(self, scene);

        if (Switch.CheckLevelFlag(self.SceneAs<Level>()))
        {
            TrySetFlagThroughEntity(self);
        }
    }

    private static void ILHookSwitchGateSequenceCoroutine(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        if (cursor.TryGotoNext(
                ins => ins.MatchLdloc1(),
                ins => ins.MatchLdfld(typeof(SwitchGate).GetField("persistent", BindingFlags.NonPublic | BindingFlags.Instance))
            ))
        {
            cursor.EmitLdloc1();
            cursor.EmitDelegate<Action<SwitchGate>>(switchGate => { TrySetFlagThroughEntity(switchGate); });
        }
    }
}