using Celeste.Mod.Entities;
using LuckyHelper.Extensions;
using LuckyHelper.Module;

namespace LuckyHelper.Entities;

[CustomEntity("LuckyHelper/TimerText")]
[Tracked]
public class TimerText : ShowDataText
{
    private string format;

    public TimerText(EntityData data, Vector2 offset) : base(data, offset)
    {
        format = data.Attr("format", @"mm\:ss\:ff");
    }

    protected override string GetContent()
    {
        return showType switch
        {
            ShowTypes.CurrentRoom => TimeSpan.FromSeconds(LuckyHelperModule.Session.CurrentRoomTime.GetFloat(currentRoom)).ToString(format),
            ShowTypes.FromSavedPath => TimeSpan.FromSeconds(LuckyHelperModule.Session.SavedPathTime.GetFloat(savedPath)).ToString(format),
            ShowTypes.SinceStart => TimeSpan.FromSeconds(LuckyHelperModule.Session.TotalTime).ToString(format),
            ShowTypes.SinceLastCheckpoint => TimeSpan
                .FromSeconds(LuckyHelperModule.Session.CurrentCheckpointTime.GetFloat(LuckyHelperModule.Session.PlayerLastCheckPoint))
                .ToString(format),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    protected override void ClearData()
    {
        switch (showType)
        {
            case ShowTypes.CurrentRoom:
                LuckyHelperModule.Session.CurrentRoomTime[currentRoom] = 0;
                break;
            case ShowTypes.FromSavedPath:
                LuckyHelperModule.Session.SavedPathTime[savedPath] = 0;
                break;
            case ShowTypes.SinceLastCheckpoint:
                LuckyHelperModule.Session.CurrentCheckpointTime[LuckyHelperModule.Session.PlayerLastCheckPoint] = 0;
                break;
            case ShowTypes.SinceStart:
                LuckyHelperModule.Session.TotalTime = 0;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}