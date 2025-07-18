using Celeste.Mod.Entities;
using LuckyHelper.Extensions;
using LuckyHelper.Module;
using LuckyHelper.Utils;
using Microsoft.Xna.Framework.Graphics;

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