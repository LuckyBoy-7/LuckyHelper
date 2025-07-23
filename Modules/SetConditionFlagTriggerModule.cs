using System.Reflection;
using Lucky.Kits.Collections;
using LuckyHelper.Extensions;
using LuckyHelper.Module;
using LuckyHelper.Triggers;
using LuckyHelper.Utils;
using Microsoft.Xna.Framework.Input;
using Mono.Cecil.Cil;
using MonoMod.RuntimeDetour;
using WindController = On.Celeste.WindController;

namespace LuckyHelper.Modules;

public class SetConditionFlagTriggerModule
{
    private static OnJumpConditionHandler onJumpHandler = new();
    private static HashSet<Tuple<string, ulong>> flagsToRemove = new();
    private static DefaultDict<SetFlagConditionType, List<SetConditionFlagTrigger>> stateTypeToTriggers = new(() => new());

    [Load]
    public static void Load()
    {
        On.Celeste.Player.Update += PlayerOnUpdate;
        On.Celeste.Player.Jump += PlayerOnJump;
        On.Celeste.Player.SuperJump += PlayerOnSuperJump;
        On.Celeste.Player.WallJump += PlayerOnWallJump;
        On.Celeste.Player.SuperWallJump += PlayerOnSuperWallJump;
        On.Celeste.Level.Update += LevelOnUpdate;
    }

    private static void PlayerOnUpdate(On.Celeste.Player.orig_Update orig, Player self)
    {
        orig(self);
        stateTypeToTriggers.Clear();
        foreach (SetConditionFlagTrigger trigger in self.CollideAll<SetConditionFlagTrigger>())
        {
            stateTypeToTriggers[trigger.flagData.ConditionType].Add(trigger);
        }
    }

    private static void LevelOnUpdate(On.Celeste.Level.orig_Update orig, Celeste.Level self)
    {
        HashSet<Tuple<string, ulong>> toRemove = new();
        foreach (var tuple in flagsToRemove)
        {
            if (tuple.Item2 == Engine.FrameCounter)
                toRemove.Add(tuple);
        }

        foreach (var tuple in toRemove)
        {
            flagsToRemove.Remove(tuple);
            self.Session.SetFlag(tuple.Item1, false);
        }

        orig(self);
    }


    [Unload]
    public static void Unload()
    {
        On.Celeste.Player.Update -= PlayerOnUpdate;
        On.Celeste.Player.Jump -= PlayerOnJump;
        On.Celeste.Player.SuperJump -= PlayerOnSuperJump;
        On.Celeste.Player.WallJump -= PlayerOnWallJump;
        On.Celeste.Player.SuperWallJump -= PlayerOnSuperWallJump;
        On.Celeste.Level.Update -= LevelOnUpdate;
    }

    private static void PlayerOnWallJump(On.Celeste.Player.orig_WallJump orig, Player self, int dir)
    {
        onJumpHandler.OnConditionFit(self);
        orig(self, dir);
    }

    private static void PlayerOnSuperJump(On.Celeste.Player.orig_SuperJump orig, Player self)
    {
        onJumpHandler.OnConditionFit(self);
        orig(self);
    }

    private static void PlayerOnJump(On.Celeste.Player.orig_Jump orig, Player self, bool particles, bool playSfx)
    {
        onJumpHandler.OnConditionFit(self);
        orig(self, particles, playSfx);
    }


    private static void PlayerOnSuperWallJump(On.Celeste.Player.orig_SuperWallJump orig, Player self, int dir)
    {
        onJumpHandler.OnConditionFit(self);
        orig(self, dir);
    }


    public abstract class SetConditionFlagTriggerHandler
    {
        public abstract SetFlagConditionType StateType { get; }

        public void OnConditionFit(Player player)
        {
            if (!TryGetTriggerData(out var data))
                return;

            SetFlag(player.Session(), data);
        }

        private static void SetFlag(Session session, SetConditionFlagTriggerData data)
        {
            string flag = data.Flag;
            session.SetFlag(flag);
            if (data.RemoveFlagDelayedFrames > 0)
                flagsToRemove.Add(new Tuple<string, ulong>(flag, Engine.FrameCounter + (ulong)data.RemoveFlagDelayedFrames));
        }

        private bool TryGetTriggerData(out SetConditionFlagTriggerData data)
        {
            var stateToDatas = LuckyHelperModule.Session.SetConditionFlagTriggerStateToDatas;
            data = new();

            var collidedTriggers = stateTypeToTriggers[StateType];
            var stayModeTrigger = collidedTriggers.FirstOrDefault(trigger => trigger.flagData.ActivationType == ActivationType.Stay, null);
            if (stayModeTrigger != null)
            {
                data = stayModeTrigger.flagData;
                return true;
            }

            if (stateToDatas.TryGetValue(StateType, out var tmpData) && tmpData.ActivationType == ActivationType.Set)
            {
                data = tmpData;
                return true;
            }

            return false;
        }
    }

    public class OnJumpConditionHandler : SetConditionFlagTriggerHandler
    {
        public override SetFlagConditionType StateType => SetFlagConditionType.OnJump;
    }
}