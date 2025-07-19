namespace LuckyHelper.Utils;

public struct RenderedBoundsByColliderTypeData
{
    public ColliderType ColliderType;
    public Vector2 Position;
    public float Radius;
    public float Width;
    public float Height;
    public Color InnerColor;
    public Color BorderColor;
    public float Alpha;
    public bool ShowBackground;
    public bool ShowBorder;
}

public static class RenderUtils
{
    public static void RenderBoundsByColldierType(RenderedBoundsByColliderTypeData data)
    {
        if (data.ColliderType == ColliderType.Circle)
        {
            Vector2 start = data.Position - new Vector2(data.Radius, data.Radius);
            if (data.ShowBackground)
                Draw.Rect(start, data.Radius * 2, data.Radius * 2, data.InnerColor * data.Alpha);
            if (data.ShowBorder)
                Draw.HollowRect(start, data.Radius * 2, data.Radius * 2, data.BorderColor * data.Alpha);
        }
        else if (data.ColliderType == ColliderType.Rectangle)
        {
            Vector2 start = data.Position - new Vector2(data.Width / 2, data.Height / 2);
            if (data.ShowBackground)
                Draw.Rect(start, data.Width, data.Height, data.InnerColor * data.Alpha);
            if (data.ShowBorder)
                Draw.HollowRect(start, data.Width, data.Height, data.BorderColor * data.Alpha);
        }
    }
}