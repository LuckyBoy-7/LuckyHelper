using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices.JavaScript;
using Celeste.Mod.Entities;
using LuckyHelper.Extensions;
using LuckyHelper.Module;
using LuckyHelper.Modules;
using Player = On.Celeste.Player;

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

}