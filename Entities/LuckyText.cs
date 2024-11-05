using LuckyHelper.Extensions;

namespace LuckyHelper.Entities;

public class LuckyText : Entity
{
    private float scale;
    private float alpha;
    private Color color;

    private float duration;
    private string format;

    private bool outline;
    private float thickness;
    private Color outlineColor;
    public virtual string Content { get; set; }

    public LuckyText(EntityData data, Vector2 offset)
        : base(data.Position + offset)
    {
        scale = data.Float("scale");
        alpha = data.Float("alpha");
        color = data.HexColor("color");
        outline = data.Bool("outline");
        thickness = data.Float("thickness");
        outlineColor = data.HexColor("outlineColor");
    }


    public override void Render()
    {
        Camera camera = SceneAs<Level>().Camera;
        Vector2 pos = (Position - camera.Position) * 6;


        if (outline)
            ActiveFont.DrawOutline(Content, pos, new Vector2(0f, 0f), Vector2.One * scale, color.WithA(alpha), thickness, outlineColor.WithA(alpha));
        else
            ActiveFont.Draw(Content, pos, new Vector2(0f, 0f), Vector2.One * scale, color.WithA(alpha));
    }
}