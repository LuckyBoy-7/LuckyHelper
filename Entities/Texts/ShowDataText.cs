using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices.JavaScript;
using Celeste.Mod.Entities;
using LuckyHelper.Extensions;
using LuckyHelper.Module;
using LuckyHelper.Modules;
using Player = On.Celeste.Player;

namespace LuckyHelper.Entities;

public abstract class ShowDataText : LuckyText
{
    public enum ShowTypes
    {
        CurrentRoom,
        SinceStart,
        FromSavedPath,
        SinceLastCheckpoint,
    }

    protected ShowTypes showType;

    public override sealed string Content
    {
        get
        {
            if (remainCurrentValue && remainedContent != "-1")
                return remainedContent;
            remainedContent = GetContent();
            return remainedContent;
        }
    }

    protected abstract string GetContent();


    protected string currentRoom;
    protected string savedPath;
    protected bool remainCurrentValue;
    protected string remainedContent = "-1";

    public ShowDataText(EntityData data, Vector2 offset) : base(data, offset)
    {
        Tag = Tags.HUD | Tags.TransitionUpdate;
        showType = data.Enum<ShowTypes>("showType");
        remainCurrentValue = data.Bool("remainCurrentValue");
        savedPath = data.Attr("savedPath");
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        currentRoom = this.CurrentRoomName();
    }
}