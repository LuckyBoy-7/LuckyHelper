using Celeste.Mod.Entities;
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

        sprite = new Sprite(GFX.Game, "LuckyHelper/pass_by_refill/");
        Add(sprite);
        sprite.AddLoop("idle", "img", 1);
        sprite.Play("idle", true);
        // sprite.Effects = SpriteEffects.FlipHorizontally;
        sprite.Origin = new Vector2(64, 64);
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
}