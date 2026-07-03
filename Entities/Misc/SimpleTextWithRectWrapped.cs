using LuckyHelper.Utils;

namespace LuckyHelper.Entities.Misc;

public class SimpleTextWithRectWrapped : Entity
{
    private SimpleText simpleText;

    private string Content
    {
        get => simpleText.Content;
        set => simpleText.Content = value;
    }

    public float Scale
    {
        get => simpleText.Scale;
        set => simpleText.Scale = value;
    }

    public Color InnerColor
    {
        get => simpleText.InnerColor;
        set => simpleText.InnerColor = value;
    }

    public Color OutlineColor
    {
        get => simpleText.OutlineColor;
        set => simpleText.OutlineColor = value;
    }

    public float Alpha
    {
        get => simpleText.Alpha;
        set => simpleText.Alpha = value;
    }

    public Vector2 Justify
    {
        get => simpleText.Justify;
        set => simpleText.Justify = value;
    }

    public bool UseScreenPosition
    {
        get => simpleText.UseScreenPosition;
        set => simpleText.UseScreenPosition = value;
    }

    public Color RectColor = Color.Black;
    public float Padding;


    public SimpleTextWithRectWrapped(string content)
    {
        Tag = Tags.HUD;
        simpleText = new SimpleText(content);
        Content = content;
    }


    public override void Render()
    {
        simpleText.Position = Position;
        
        Vector2 origDrawTextPosition = simpleText.ScreenPosition(SceneAs<Level>());
        float width = simpleText.FontWidth + Padding * 2;
        float height = simpleText.FontHeight + Padding * 2;
        Vector2 rectStartPosition = origDrawTextPosition - Justify * new Vector2(width, height);
        Draw.Rect(rectStartPosition, width, height, RectColor * Alpha);
        
        
        Vector2 offset = (new Vector2(0.5f, 0.5f) - Justify) * 2 * Padding;
        simpleText.Position += offset;
        simpleText.RawRender(SceneAs<Level>());
    }
}