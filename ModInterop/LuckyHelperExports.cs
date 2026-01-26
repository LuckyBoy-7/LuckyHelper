using LuckyHelper.Entities.Misc;
using MonoMod.ModInterop;

namespace LuckyHelper.ModInterop;

[ModExportName("LuckyHelper")]
public static class LuckyHelperExports
{
    public static List<Player> GetDummyPlayers()
    {
        if (Engine.Scene is Level level)
        {
            return level.Tracker.GetEntities<DummyPlayer>().Cast<Player>().ToList();
        }

        return null;
    }
}