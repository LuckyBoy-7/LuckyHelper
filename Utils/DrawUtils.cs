namespace LuckyHelper.Utils;

public static class DrawUtils
{
    public static void DottedLine(Vector2 start, Vector2 end, Color color, int segments = 5)
    {
        if (segments % 2 == 0)
            segments += 1;
        for (int i = 0; i < segments; i += 2)
        {
            Vector2 p1 = Vector2.Lerp(start, end, (float)i / segments);
            Vector2 p2 = Vector2.Lerp(start, end, (float)(i + 1) / segments);
            Draw.Line(p1, p2, color);
        }
    }
}