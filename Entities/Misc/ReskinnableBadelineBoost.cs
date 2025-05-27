using Celeste.Mod.Entities;
using LuckyHelper.Extensions;
using LuckyHelper.Module;
using Microsoft.Xna.Framework.Graphics;

namespace LuckyHelper.Entities;

[CustomEntity("LuckyHelper/ReskinnableBadelineBoost")]
[Tracked]
public class ReskinnableBadelineBoost : BadelineBoost
{
    public ReskinnableBadelineBoost(Vector2[] nodes, bool lockCamera, bool canSkip = false, bool finalCh9Boost = false, bool finalCh9GoldenBoost = false, bool finalCh9Dialog = false) : base(nodes, lockCamera, canSkip, finalCh9Boost, finalCh9GoldenBoost, finalCh9Dialog)
    {
    }

    public ReskinnableBadelineBoost(EntityData data, Vector2 offset) : base(data, offset)
    {
        Remove(sprite);
        sprite = GFX.SpriteBank.Create(data.Attr("XMLId"));
        Add(sprite);
        stretch.Texture = GFX.Game[data.Attr("stretchTexturePath")];
    }
}