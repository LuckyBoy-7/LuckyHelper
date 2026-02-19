namespace LuckyHelper.Entities.Misc;

public class SimpleText : Entity
{
    private string text;
    public float Scale = 1f;

    public SimpleText(string text) 
    {
        Tag = Tags.HUD;
        this.text = text;
    }


    public override void Render()
    {
        Vector2 cameraPosition = SceneAs<Level>().Camera.Position;
        Vector2 screenPosition = (Position - cameraPosition) * 6f;
        if (SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode)
        {
            screenPosition.X = 1920f - screenPosition.X;
        }

        ActiveFont.DrawOutline(text, screenPosition, new Vector2(0.5f, 0.5f), Vector2.One * Scale, Color.White, 1, Color.Black);
    }
}