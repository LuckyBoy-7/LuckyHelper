using LuckyHelper.Extensions;
using LuckyHelper.Module;
using LuckyHelper.Triggers;

namespace LuckyHelper.Modules;

public class DeathCountModule
{
    [Load]
    public static void Load()
    {
        On.Celeste.Player.Die += PlayerOnDie;
    }

    [Unload]
    public static void Unload()
    {
        On.Celeste.Player.Die -= PlayerOnDie;
    }

    private static PlayerDeadBody PlayerOnDie(On.Celeste.Player.orig_Die orig, Celeste.Player self, Vector2 direction, bool evenifinvincible,
        bool registerdeathinstats)
    {
        var session = self.SceneAs<Level>().Session;


        session.IncrementCounter(TimerModule.CurrentCheckpoint + "DeathCount");
        session.IncrementCounter(session.Level + "DeathCount");
        session.IncrementCounter("TotalDeathCount");

        DeathCountSavedIn pos = self.GetEntity<DeathCountSavedIn>();
        if (pos != null)
        {
            session.IncrementCounter(pos.SavedIn + "DeathCount");
        }

        return orig(self, direction, evenifinvincible, registerdeathinstats);
    }
}