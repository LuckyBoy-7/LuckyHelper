using System.Reflection;
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

    [Load]
    public static void Load()
    {
        On.Celeste.Player.Jump += PlayerOnJump;
        On.Celeste.Player.SuperJump += PlayerOnSuperJump;
        On.Celeste.Player.WallJump += PlayerOnWallJump;
        On.Celeste.Player.SuperWallJump += PlayerOnSuperWallJump;
        On.Celeste.Level.Update += LevelOnUpdate;
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
        On.Celeste.Player.Jump -= PlayerOnJump;
        On.Celeste.Player.SuperJump -= PlayerOnSuperJump;
        On.Celeste.Player.WallJump -= PlayerOnWallJump;
        On.Celeste.Player.SuperWallJump -= PlayerOnSuperWallJump;
        On.Celeste.Level.Update -= LevelOnUpdate;
    }

    private static void PlayerOnWallJump(On.Celeste.Player.orig_WallJump orig, Player self, int dir)
    {
        onJumpHandler.OnTriggered(self);
        orig(self, dir);
    }

    private static void PlayerOnSuperJump(On.Celeste.Player.orig_SuperJump orig, Player self)
    {
        onJumpHandler.OnTriggered(self);
        orig(self);
    }

    private static void PlayerOnJump(On.Celeste.Player.orig_Jump orig, Player self, bool particles, bool playSfx)
    {
        onJumpHandler.OnTriggered(self);
        orig(self, particles, playSfx);
    }


    private static void PlayerOnSuperWallJump(On.Celeste.Player.orig_SuperWallJump orig, Player self, int dir)
    {
        onJumpHandler.OnTriggered(self);
        orig(self, dir);
    }


    public abstract class SetConditionFlagTriggerHandler
    {
        public abstract SetConditionFlagTriggerStateType StateType { get; }

        public void OnTriggered(Player player)
        {
            var stateToDatas = LuckyHelperModule.Session.SetConditionFlagTriggerStateToDatas;
            if (!stateToDatas.TryGetValue(StateType, out var data) || !data.On)
                return;

            string flag = data.Flag;
            player.Session().SetFlag(flag);
            if (data.RemoveFlagDelayedFrames > 0)
                flagsToRemove.Add(new Tuple<string, ulong>(flag, Engine.FrameCounter + (ulong)data.RemoveFlagDelayedFrames));
        }
    }

    public class OnJumpConditionHandler : SetConditionFlagTriggerHandler
    {
        public override SetConditionFlagTriggerStateType StateType => SetConditionFlagTriggerStateType.OnJump;
    }
}