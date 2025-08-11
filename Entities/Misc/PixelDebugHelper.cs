using Celeste.Mod.Entities;
using Microsoft.Xna.Framework.Input;

namespace LuckyHelper.Entities;

[CustomEntity("LuckyHelper/PixelDebugHelper")]
[Tracked]
public class PixelDebugHelper : Solid
{
    private const int SIZE = 24;
    public PixelDebugHelper(Vector2 position, int dashes) : base(Vector2.Zero, SIZE, SIZE, true)
    {
        Depth = 1;
    }

    public PixelDebugHelper(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Int("dashes"))
    {
    }

    public override void Update()
    {
        base.Update();

        if (MInput.Keyboard.Check(Keys.LeftControl))
        {
            if (MInput.Keyboard.Pressed(Keys.OemMinus))
            {
                Player player = Scene.Tracker.GetEntity<Player>();
                if (player != null)
                    Position = player.Position;
            }
            
            if (MInput.Keyboard.Pressed(Keys.OemPlus))
            {
                Player player = Scene.Tracker.GetEntity<Player>();
                if (player != null)
                    Position = player.Position + (SIZE + 11) * -Vector2.UnitY;
            }


            // if (MInput.Keyboard.Pressed(Keys.Right))
            //     Position += new Vector2(1, 0);
            // if (MInput.Keyboard.Pressed(Keys.Left))
            //     Position += new Vector2(-1, 0);
            // if (MInput.Keyboard.Pressed(Keys.Down))
            //     Position += new Vector2(0, 1);
            // if (MInput.Keyboard.Pressed(Keys.Up))
            //     Position += new Vector2(0, -1);

            // if (MInput.Keyboard.Pressed(Keys.Right))
            //     throw new Exception("test");
            if (MInput.Keyboard.Pressed(Keys.Right))
                MoveH(1);
            if (MInput.Keyboard.Pressed(Keys.Left))
                MoveH(-1);
            if (MInput.Keyboard.Pressed(Keys.Down))
                MoveV(1);
            if (MInput.Keyboard.Pressed(Keys.Up))
                MoveV(-1);
        }
    }

    public override void Render()
    {
        base.Render();
        Draw.Rect(Position, Width, Height, Color.Gray);
    }
}