using Celeste.Mod.Entities;
using LuckyHelper.Extensions;
using Timer = LuckyHelper.Utils.Timer;

namespace LuckyHelper.Entities.Texts;

[CustomEntity("LuckyHelper/FilmTextPivot")]
[Tracked]
public class FilmTextPivot : Entity
{
    public FilmTextPivot(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
    }
}