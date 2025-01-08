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
        LuckyHelperModule.Session.CurrentCheckpointDeathCount.AddInt(LuckyHelperModule.Session.PlayerLastCheckPoint, 1);
        LuckyHelperModule.Session.CurrentRoomDeathCount.AddInt(self.CurrentRoomName(), 1);
        LuckyHelperModule.Session.TotalDeathCount += 1;

        DeathCountSavedIn savedPath = self.GetEntity<DeathCountSavedIn>();
        if (savedPath != null)
        {
            LuckyHelperModule.Session.SavedPathDeathCount.AddInt(savedPath.SavedIn, 1);
        }

        return orig(self, direction, evenifinvincible, registerdeathinstats);
    }
}