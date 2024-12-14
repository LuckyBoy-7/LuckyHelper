using LuckyHelper.Extensions;
using LuckyHelper.Module;
using LuckyHelper.Triggers;
using Player = On.Celeste.Player;

namespace LuckyHelper.Modules;

public class TimerModule
{
    public const int Resolution = 100000;

    public const string TimerModuleHash = "TimerModuleHash";
    public static string CurrentCheckpoint;

    [Load]
    public static void Load()
    {
        On.Celeste.Player.Update += PlayerOnUpdate;
    }


    [Unload]
    public static void Unload()
    {
        On.Celeste.Player.Update -= PlayerOnUpdate;
    }

    private static void PlayerOnUpdate(Player.orig_Update orig, Celeste.Player self)
    {
        orig(self);
        var session = self.SceneAs<Level>().Session;

        if (!self.Session().GetFlag(TimerModuleHash))
        {
            self.Session().SetFlag(TimerModuleHash);
            CurrentCheckpoint = "StartCheckpoint";
        }

        string name = self.GetCheckpointName();
        if (CurrentCheckpoint != name && name != "")  // 碰到新的就切换过去, 除了start
        {
            CurrentCheckpoint = name;
        }

        session.SetCounter(CurrentCheckpoint + "/Lucky/Timer", (int)(session.GetCounter(CurrentCheckpoint + "/Lucky/Timer") + Engine.RawDeltaTime * Resolution));
        session.SetCounter(session.Level + "/Lucky/Timer", (int)(session.GetCounter(session.Level + "/Lucky/Timer") + Engine.RawDeltaTime * Resolution));
        TimeSavedIn pos = self.GetEntity<TimeSavedIn>();
        if (pos != null)
        {
            session.SetCounter(pos.SavedIn + "Lucky/Timer", (int)(session.GetCounter(pos.SavedIn + "Lucky/Timer") + Engine.RawDeltaTime * Resolution));
        }

    }
}