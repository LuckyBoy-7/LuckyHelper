using Celeste.Mod.Entities;
using LuckyHelper.Utils;

namespace LuckyHelper.Entities.Misc;

[CustomEntity("LuckyHelper/CrystalHeartDialogController")]
[Tracked]
public class CrystalHeartDialogController : Entity
{
    public List<string> Dialogs;

    public CrystalHeartDialogController(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        Dialogs = ParseUtils.ParseCommaSeperatedStringToList(data.Attr("dialogs"));
    }
}