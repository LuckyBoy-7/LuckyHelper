using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices.JavaScript;
using Celeste.Mod.Entities;
using LuckyHelper.Extensions;
using Player = On.Celeste.Player;

namespace LuckyHelper.Entities;

[CustomEntity("LuckyHelper/DeathCountText")]
public class DeathCountText : LuckyText
{
    private bool isShowTotalDeath;

    public override string Content
    {
        get
        {
            if (isShowTotalDeath)
                return SceneAs<Level>().Session.GetCounter("TotalDeathCount").ToString();
            return SceneAs<Level>().Session.GetCounter(id).ToString();
        }
    }

    public string id;

    public DeathCountText(EntityData data, Vector2 offset) : base(data, offset)
    {
        Tag = Tags.HUD | Tags.TransitionUpdate;
        isShowTotalDeath = data.Bool("isShowTotalDeath");
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        id = SceneAs<Level>().Session.LevelData.Name + "DeathCount";
    }
}