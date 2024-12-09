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
        ReadFromSavedPath
    }

    private ShowTypes showType;

    public override string Content
    {
        get
        {
            switch (showType)
            {
                case ShowTypes.CurrentRoom:
                    return TimeSpan.FromSeconds((float)SceneAs<Level>().Session.GetCounter(this.Session().Level + "/Lucky/Timer") / TimerModule.Resolution).ToString(format);
                case ShowTypes.TotalTimeFromStart:
                    return TimeSpan.FromSeconds((float)SceneAs<Level>().Session.Time / 10_000_000).ToString(format);
                case ShowTypes.ReadFromSavedPath:
                    return TimeSpan.FromSeconds((float)SceneAs<Level>().Session.GetCounter(savedPath + "Lucky/Timer") / TimerModule.Resolution).ToString(format);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private string savedPath;
    private string format;


    public TimerText(EntityData data, Vector2 offset) : base(data, offset)
    {
        Tag = Tags.HUD | Tags.TransitionUpdate;

        format = data.Attr("format", @"mm\:ss\:ff");
        savedPath = data.Attr("savedPath");
        showType = data.Enum<ShowTypes>("showType");
    }
}