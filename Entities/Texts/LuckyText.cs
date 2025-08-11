namespace LuckyHelper.Entities;

public class LuckyText : Entity
{
    private float scale;
    private float alpha;
    private Color color;

    private bool outline;
    private float thickness;
    private Color outlineColor;
    public virtual string Content { get; set; }
    private bool hiddenOnPause;
    private bool useEnglishFontAlways;

    public LuckyText(EntityData data, Vector2 offset)
        : base(data.Position + offset)
    {
        scale = data.Float("scale");
        alpha = data.Float("alpha");
        color = data.HexColor("color");
        outline = data.Bool("outline");
        thickness = data.Float("thickness");
        outlineColor = data.HexColor("outlineColor");
        hiddenOnPause = data.Bool("hiddenOnPause");
        useEnglishFontAlways = data.Bool("useEnglishFontAlways");
    }


    public override void Render()
    {
        Camera camera = SceneAs<Level>().Camera;
        Vector2 pos = (Position - camera.Position) * 6;

        // 暂停时隐藏
        if (SceneAs<Level>().Paused && hiddenOnPause)
            return;

        // 使用英文字体
        Language origDialogLanguage = Dialog.Language;
        if (useEnglishFontAlways)
            Dialog.Language = Dialog.Languages["english"];

        if (outline)
            ActiveFont.DrawOutline(Content, pos, new Vector2(0f, 0f), Vector2.One * scale, color, thickness, outlineColor * alpha);
        else
            ActiveFont.Draw(Content, pos, new Vector2(0f, 0f), Vector2.One * scale, color * alpha);

        if (useEnglishFontAlways)
            Dialog.Language = origDialogLanguage;
    }
}