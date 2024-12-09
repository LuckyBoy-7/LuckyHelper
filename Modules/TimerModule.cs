using LuckyHelper.Extensions;
using LuckyHelper.Module;
using LuckyHelper.Triggers;
using Player = On.Celeste.Player;

namespace LuckyHelper.Modules;

public class TimerModule
{
    public const int Resolution = 100000;

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
        var session = self.SceneAs<Level>().Session;

        string id = session.Level + "/Lucky/Timer";
        session.SetCounter(id, (int)(session.GetCounter(id) + Engine.RawDeltaTime * Resolution));
        TimeSavedIn pos = self.GetEntity<TimeSavedIn>();
        if (pos != null)
        {
            session.SetCounter(pos.SavedIn + "Lucky/Timer", (int)(session.GetCounter(pos.SavedIn + "Lucky/Timer") + Engine.RawDeltaTime * Resolution));
        }

        orig(self);
    }
}