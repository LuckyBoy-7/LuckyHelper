using System.Reflection;
using LuckyHelper.Extensions;
using LuckyHelper.Module;
using LuckyHelper.Utils;
using MonoMod.RuntimeDetour;
using Player = On.Celeste.Player;
using WindController = On.Celeste.WindController;

namespace LuckyHelper.Modules;

public class TestModule
{

    [Load]
    public static void Load()
    {
        On.Celeste.Player.Update += PlayerOnUpdate;
        On.Celeste.WindController.Update += WindControllerOnUpdate;
    }

    [Unload]
    public static void Unload()
    {
        On.Celeste.Player.Update -= PlayerOnUpdate;
        On.Celeste.WindController.Update -= WindControllerOnUpdate;
    }

    private static void WindControllerOnUpdate(WindController.orig_Update orig, Celeste.WindController self)
    {
        orig(self);
        // Logger.Log(LogLevel.Info, "Test", "Wind " + FieldUtils.GetField(self, "actualDepth"));
    }

    private static void PlayerOnUpdate(Player.orig_Update orig, Celeste.Player self)
    {
        orig(self);
        // Logger.Warn("Test", self.GetCheckpointName());
        // Logger.Warn("Test", TimerModule.CurrentCheckpoint);
        // Logger.Log(LogLevel.Warn, "Test", self.Position.ToString());
        // float timer = self.SceneAs<Level>().Session.Time;
        // Logger.Log(LogLevel.Info, "Test", timer.ToString());
        // Logger.Log(LogLevel.Info, "Test", "Player " + FieldUtils.GetField(self, "actualDepth"));
    }
}