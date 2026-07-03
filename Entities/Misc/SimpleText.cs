namespace LuckyHelper.Entities.Misc;

public class SimpleText : Entity
{
    public string Content;
    public float Scale = 1f;
    public Color InnerColor = Color.White;
    public Color OutlineColor = Color.White;
    public float Alpha = 1f;
    public Vector2 Justify = new Vector2(0.5f, 0.5f);

    public bool UseScreenPosition;

    public Vector2 ScreenPosition(Level level)
    {
        Vector2 cameraPosition = level.Camera.Position;
        Vector2 screenPosition = (Position - cameraPosition) * 6f;
        if (UseScreenPosition)
            screenPosition = Position;
        if (SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode)
        {
            screenPosition.X = 1920f - screenPosition.X;
        }

        return screenPosition;
    }

    public float FontWidth => ActiveFont.Measure(Content).X * Scale;
    public float FontHeight => ActiveFont.Measure(Content).Y * Scale;


    public SimpleText(string content)
    {
        Tag = Tags.HUD;
        Content = content;
    }


    public override void Render()
    {
        RawRender(SceneAs<Level>());
    }

    public void RawRender(Level level)
    {
        ActiveFont.DrawOutline(Content, ScreenPosition(level), Justify, Vector2.One * Scale, InnerColor * Alpha, 1, OutlineColor * Alpha);
    }
}