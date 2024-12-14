using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices.JavaScript;
using Celeste.Mod.Entities;
using LuckyHelper.Extensions;
using LuckyHelper.Modules;

namespace LuckyHelper.Entities;

[CustomEntity("LuckyHelper/TimerText")]
[Tracked]
public class TimerText : LuckyText
{
    public enum ShowTypes
    {
        CurrentRoom,
        TotalTimeFromStart,
        ReadFromSavedPath,
        CurrentCheckpoint
    }

    private ShowTypes showType;

    public override string Content
    {
        get
        {
            switch (showType)
            {
                case ShowTypes.CurrentRoom:
                case ShowTypes.ReadFromSavedPath:
                case ShowTypes.CurrentCheckpoint:
                    return TimeSpan.FromSeconds((float)SceneAs<Level>().Session.GetCounter(readTimeFrom) / TimerModule.Resolution).ToString(format);
                case ShowTypes.TotalTimeFromStart:
                    return TimeSpan.FromSeconds((float)SceneAs<Level>().Session.Time / 10_000_000).ToString(format);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private string savedPath;
    private string format;
    private string readTimeFrom;


    public TimerText(EntityData data, Vector2 offset) : base(data, offset)
    {
        Tag = Tags.HUD | Tags.TransitionUpdate;

        format = data.Attr("format", @"mm\:ss\:ff");
        savedPath = data.Attr("savedPath");
        showType = data.Enum<ShowTypes>("showType");
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);

        // Logger.Warn("Test", TimerModule.CurrentCheckpoint);
        // Logger.Warn("Test", "-------------------------");
        readTimeFrom = showType switch
        {
            ShowTypes.CurrentRoom => this.Session().Level + "/Lucky/Timer",
            ShowTypes.TotalTimeFromStart => "-1",
            ShowTypes.ReadFromSavedPath => savedPath + "Lucky/Timer",
            // todo: 由于checkpoint更新会慢一帧, 所以timertext不能和checkpoint放一个房间
            ShowTypes.CurrentCheckpoint => TimerModule.CurrentCheckpoint + "/Lucky/Timer",
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}