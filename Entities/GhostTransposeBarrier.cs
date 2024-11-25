using Celeste.Mod.Entities;

namespace LuckyHelper.Entities;

[Tracked]
[CustomEntity("LuckyHelper/GhostTransposeBarrier")]
public class GhostTransposeBarrier : Solid
{
    private static bool disableRefill;

    private readonly List<GhostTransposeBarrier> adjacent = new List<GhostTransposeBarrier>();
    private readonly List<Vector2> particles = new List<Vector2>();
    private readonly float[] speeds = { 12f, 20f, 40f };
    private readonly MTexture temp;
    private float flash;
    private bool flashing;
    private float offX;
    private float offY;
    private Color backgroundColor;
    private Color particleColor;

    private GhostTransposeBarrier(Vector2 position, float width, float height)
        : base(position, width, height, false)
    {
        Collidable = false;
        temp = new MTexture();
        for (var index = 0; (double)index < (double)Width * (double)Height / 16.0; ++index)
        {
            particles.Add(new Vector2(Calc.Random.NextFloat(Width - 1f), Calc.Random.NextFloat(Height - 1f)));
        }

        offX = position.X;
        offY = position.Y;
        while (offX < 0.0)
        {
            offX += 128f;
        }

        while (offY < 0.0)
        {
            offY += 128f;
        }

        Add(new DisplacementRenderHook(RenderDisplacement));
    }

    public GhostTransposeBarrier(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Width, data.Height)
    {
        backgroundColor = data.HexColor("backgroundColor");
        particleColor = data.HexColor("particleColor");
    }
    

    public override void Update()
    {
        offX += Engine.DeltaTime * 12f;
        offY += Engine.DeltaTime * 12f;
        if (flashing)
        {
            flash = Calc.Approach(flash, 0.0f, Engine.DeltaTime * 5f);
            if (flash <= 0.0)
            {
                flashing = false;
            }
        }

        var length = speeds.Length;
        var index = 0;
        for (var count = particles.Count; index < count; ++index)
        {
            Vector2 vector2 = particles[index] + Vector2.UnitY * speeds[index % length] * Engine.DeltaTime;
            vector2.Y %= Height - 1f;
            particles[index] = vector2;
        }

        base.Update();
    }


    private void RenderDisplacement()
    {
        MTexture mTexture = GFX.Game["util/displacementBlock"];
        Color color = Color.White * 0.3f;
        for (var index1 = 0; (double)index1 < (double)Width; index1 += 128)
        for (var index2 = 0; (double)index2 < (double)Height; index2 += 128)
        {
            mTexture.GetSubtexture(
                (int)(offX % 128.0), (int)(offY % 128.0),
                (int)Math.Min(128f, Width - index1), (int)Math.Min(128f, Height - index2), temp
            );
            temp.Draw(Position + new Vector2(index1, index2), Vector2.Zero, color);
        }
    }

    public override void Render()
    {
        Draw.Rect(Collider, backgroundColor * 0.2f);
        if (flash > 0.0)
        {
            Draw.Rect(Collider, backgroundColor * flash);
        }

        foreach (Vector2 particle in particles)
        {
            Draw.Pixel.Draw(Position + particle, Vector2.Zero, particleColor);
        }
    }

}