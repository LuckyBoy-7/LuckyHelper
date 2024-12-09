using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices.JavaScript;
using Celeste.Mod.Entities;
using LuckyHelper.Extensions;
using Player = On.Celeste.Player;

namespace LuckyHelper.Entities;

[CustomEntity("LuckyHelper/DeathCountText")]
public class DeathCountText : LuckyText
{
    public enum ShowTypes
    {
        CurrentRoom,
        TotalDeathFromStart,
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
                    return SceneAs<Level>().Session.GetCounter(id).ToString();
                case ShowTypes.TotalDeathFromStart:
                    return SceneAs<Level>().Session.GetCounter("TotalDeathCount").ToString();
                case ShowTypes.ReadFromSavedPath:
                    return SceneAs<Level>().Session.GetCounter(savedPath + "DeathCount").ToString();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public string id;
    private string savedPath;

    public DeathCountText(EntityData data, Vector2 offset) : base(data, offset)
    {
        Tag = Tags.HUD | Tags.TransitionUpdate;
        showType = data.Enum<ShowTypes>("showType");
        savedPath = data.Attr("savedPath");
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        id = SceneAs<Level>().Session.LevelData.Name + "DeathCount";
    }
}