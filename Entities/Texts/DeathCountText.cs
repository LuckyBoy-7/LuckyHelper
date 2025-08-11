using Celeste.Mod.Entities;
using LuckyHelper.Extensions;
using LuckyHelper.Module;

namespace LuckyHelper.Entities;

[CustomEntity("LuckyHelper/DeathCountText")]
public class DeathCountText : ShowDataText
{
    public DeathCountText(EntityData data, Vector2 offset) : base(data, offset)
    {
    }

    protected override string GetContent()
    {
        return showType switch
        {
            ShowTypes.CurrentRoom => LuckyHelperModule.Session.CurrentRoomDeathCount.GetInt(currentRoom).ToString(),
            ShowTypes.SinceStart => LuckyHelperModule.Session.TotalDeathCount.ToString(),
            ShowTypes.FromSavedPath => LuckyHelperModule.Session.SavedPathDeathCount.GetInt(savedPath).ToString(),
            ShowTypes.SinceLastCheckpoint => LuckyHelperModule.Session.CurrentCheckpointDeathCount.GetInt(LuckyHelperModule.Session.PlayerLastCheckPoint).ToString(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    protected override void ClearData()
    {
        switch (showType)
        {
            case ShowTypes.CurrentRoom:
                LuckyHelperModule.Session.CurrentRoomDeathCount[currentRoom] = 0;
                break;
            case ShowTypes.FromSavedPath:
                LuckyHelperModule.Session.SavedPathDeathCount[savedPath] = 0;
                break;
            case ShowTypes.SinceLastCheckpoint:
                LuckyHelperModule.Session.CurrentCheckpointDeathCount[LuckyHelperModule.Session.PlayerLastCheckPoint] = 0;
                break;
            case ShowTypes.SinceStart:
                LuckyHelperModule.Session.TotalDeathCount = 0;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}