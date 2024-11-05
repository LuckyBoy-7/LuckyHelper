using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices.JavaScript;
using Celeste.Mod.Entities;
using LuckyHelper.Extensions;

namespace LuckyHelper.Entities;

[CustomEntity("LuckyHelper/TimerText")]
[Tracked]
public class TimerText : LuckyText
{
    private bool isShowTotalTime;
    public string id;
    public long Duration => SceneAs<Level>().Session.GetCounter(id);

    private string format;

    public override string Content
    {
        get
        {
            if (isShowTotalTime)
                return TimeSpan.FromSeconds((float)SceneAs<Level>().Session.Time / 10000000).ToString(format);
            return TimeSpan.FromSeconds((float)Duration / Resolution).ToString(format);
        }
    }

    private const int Resolution = 100000;

    public TimerText(EntityData data, Vector2 offset) : base(data, offset)
    {
        Tag = Tags.HUD | Tags.TransitionUpdate;
        if (data.Bool("countPauseTime"))
            Tag |= Tags.PauseUpdate;

        isShowTotalTime = data.Bool("isShowTotalTime");
        format = data.Attr("format", @"mm\:ss\:ff");
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        id = SceneAs<Level>().Session.Level + "Timer" + Scene.Tracker.GetEntities<TimerText>().IndexOf(this);
    }

    public override void Update()
    {
        base.Update();
        if (!isShowTotalTime)
            SceneAs<Level>().Session.SetCounter(id, (int)(Duration + Engine.RawDeltaTime * Resolution));
    }
}