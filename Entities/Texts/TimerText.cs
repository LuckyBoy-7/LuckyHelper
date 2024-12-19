using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices.JavaScript;
using Celeste.Mod.Entities;
using LuckyHelper.Extensions;
using LuckyHelper.Module;
using LuckyHelper.Modules;

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
            ShowTypes.CurrentRoom => TimeSpan.FromSeconds(LuckyHelperModule.Session.CurrentRoomTime[currentRoom]).ToString(format),
            ShowTypes.FromSavedPath => TimeSpan.FromSeconds(LuckyHelperModule.Session.SavedPathTime[savedPath]).ToString(format),
            ShowTypes.SinceStart => TimeSpan.FromSeconds(LuckyHelperModule.Session.TotalTime).ToString(format),
            ShowTypes.SinceLastCheckpoint => TimeSpan
                .FromSeconds(LuckyHelperModule.Session.CurrentCheckpointTime[LuckyHelperModule.Session.PlayerLastCheckPoint])
                .ToString(format),
            _ => throw new ArgumentOutOfRangeException()
        };
    }


}