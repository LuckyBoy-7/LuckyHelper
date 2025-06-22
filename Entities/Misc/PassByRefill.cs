using Celeste.Mod.Entities;
using LuckyHelper.Extensions;
using LuckyHelper.Module;
using Microsoft.Xna.Framework.Graphics;

namespace LuckyHelper.Entities;

[CustomEntity("LuckyHelper/PassByRefill")]
[Tracked]
public class PassByRefill : Entity
{
    private int _dashes = 1;

    public int Dashes
    {
        get => LuckyHelperModule.Session.RoomIdToPassByRefillDahes.GetValueOrDefault(SceneAs<Level>().Session.LevelData.Name, _dashes);
        set => _dashes = value;
    }

    private Sprite sprite;

    public PassByRefill(Vector2 position, int dashes)
    {
        Depth = -1;
        Dashes = dashes;
        Position = position;
        Hitbox hitbox = new(64, 64);
        Collider = hitbox;
        // spriteName = data.Attr("xmlLabel", "SpriteEntity");
        // sprite = GFX.SpriteBank.Create(spriteName);
        // Add(sprite);

        // sprite = new Sprite(GFX.Game, "LuckyHelper/pass_by_refill/");
        // sprite.AddLoop("idle", "img", 1); 
        // sprite.Play("idle");
        // Add(sprite);
    }

    public PassByRefill(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Int("dashes"))
    {
    }

    public override void Update()
    {
        base.Update();
        var player = Scene.Tracker.GetEntity<Player>();
        if (player is not null && CollideCheck(player))
        {
            player.Dashes = Dashes;
        }
    }

    public override void Render()
    {
        base.Render();
        Draw.Rect(Position, Width, Height, Color.Red * 0.5f);
    }
}