using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices.JavaScript;
using Celeste.Mod.Entities;
using LuckyHelper.Extensions;
using LuckyHelper.Modules;
using Player = On.Celeste.Player;

namespace LuckyHelper.Entities;

[CustomEntity("LuckyHelper/DeathCountText")]
public class DeathCountText : LuckyText
{
    public enum ShowTypes
    {
        CurrentRoom,
        TotalDeathFromStart,
        ReadFromSavedPath,
        CurrentCheckpoint,
    }

    private ShowTypes showType;

    public override string Content => this.Session().GetCounter(readDeathCountFrom).ToString();

    public string id;
    private string savedPath;
    string readDeathCountFrom;

    public DeathCountText(EntityData data, Vector2 offset) : base(data, offset)
    {
        Tag = Tags.HUD | Tags.TransitionUpdate;
        showType = data.Enum<ShowTypes>("showType");
        savedPath = data.Attr("savedPath");
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);

        switch (showType)
        {
            case ShowTypes.CurrentRoom:
                readDeathCountFrom = id;
                break;
            case ShowTypes.TotalDeathFromStart:
                readDeathCountFrom = "TotalDeathCount";
                break;
            case ShowTypes.ReadFromSavedPath:
                readDeathCountFrom = savedPath + "DeathCount";
                break;
            case ShowTypes.CurrentCheckpoint:
                readDeathCountFrom = TimerModule.CurrentCheckpoint + "DeathCount";
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        id = SceneAs<Level>().Session.LevelData.Name + "DeathCount";
    }
}