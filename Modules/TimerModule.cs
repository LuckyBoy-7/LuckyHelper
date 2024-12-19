using LuckyHelper.Extensions;
using LuckyHelper.Module;
using LuckyHelper.Triggers;
using Player = On.Celeste.Player;

namespace LuckyHelper.Modules;

public class TimerModule
{
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
        LuckyHelperModule.Session.CurrentCheckpointTime[LuckyHelperModule.Session.PlayerLastCheckPoint] += Engine.DeltaTime;
        LuckyHelperModule.Session.CurrentRoomTime[self.CurrentRoomName()] += Engine.DeltaTime;
        LuckyHelperModule.Session.TotalTime += Engine.DeltaTime;

        TimeSavedIn savedPath = self.GetEntity<TimeSavedIn>();
        if (savedPath != null)
        {
            LuckyHelperModule.Session.SavedPathTime[savedPath.SavedIn] += Engine.DeltaTime;
        }

        orig(self);
    }
}