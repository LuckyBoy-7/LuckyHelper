using Celeste.Mod.Entities;
using LuckyHelper.Extensions;
using Microsoft.Xna.Framework.Graphics;

namespace LuckyHelper.Entities;

[CustomEntity("LuckyHelper/DreamZone")]
[TrackedAs(typeof(DreamBlock))]
[Tracked]
public class DreamZone : DreamBlock
{
    public bool StopPlayerOnCollide = true;
    public bool KillPlayerOnCollide = true;

    public Color BackgroundColor = Color.Green;
    public float BackgroundAlpha = 1f;

    public Color OutlineColor = Color.Black;
    public float OutlineAlpha = 0.5f;

    public string StartColors1 = "FFEF11,FF00D0,08a310";
    public string StartColors2 = "5fcde4,7fb25e,E0564C";
    public string StartColors3 = "5b6ee1,CC3B3B,7daa64";

    public bool DisableWobble = true;

    public DreamZone(EntityData data, Vector2 offset) : base(data, offset)
    {
        StopPlayerOnCollide = data.Bool("stopPlayerOnCollide");
        KillPlayerOnCollide = data.Bool("killPlayerOnCollide");

        BackgroundColor = data.HexColor("backgroundColor");
        BackgroundAlpha = data.Float("backgroundAlpha");
        OutlineColor = data.HexColor("outlineColor");
        OutlineAlpha = data.Float("outlineAlpha");

        StartColors1 = data.Attr("startColors1");
        StartColors2 = data.Attr("startColors2");
        StartColors3 = data.Attr("startColors3");

        DisableWobble = data.Bool("disableWobble");
        Collidable = false;
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        for (int i = 0; i < particles.Length; i++)
        {
            switch (particles[i].Layer)
            {
                case 0:

                    particles[i].Color = GetRandomColorByString(StartColors1);
                    break;
                case 1:
                    particles[i].Color = GetRandomColorByString(StartColors2);
                    break;
                case 2:
                    particles[i].Color = GetRandomColorByString(StartColors3);
                    break;
            }
        }
    }

    private Color GetRandomColorByString(string str)
    {
        List<string> res = new();
        string[] s = str.Split(',');
        foreach (string s1 in s)
        {
            res.Add(s1.Trim());
        }

        return Calc.HexToColor(Calc.Random.Choose(res));
    }

    public override void Render()
    {
        // base.Render();
        Camera camera = SceneAs<Level>().Camera;
        if (Right < camera.Left || Left > camera.Right || Bottom < camera.Top || Top > camera.Bottom)
        {
            return;
        }

        // background
        Color activeBackColorBackup = activeBackColor;
        Color activeLineColorBackup = activeLineColor;
        activeBackColor = BackgroundColor;
        activeLineColor = OutlineColor;
        Draw.Rect(shake.X + X, shake.Y + Y, Width, Height, (playerHasDreamDash ? activeBackColor : disabledBackColor) * BackgroundAlpha);


        Vector2 position = SceneAs<Level>().Camera.Position;
        for (int i = 0; i < particles.Length; i++)
        {
            int layer = particles[i].Layer;
            Vector2 vector = particles[i].Position;
            vector += position * (0.3f + 0.25f * layer);
            vector = PutInside(vector);
            Color color = particles[i].Color;
            MTexture mtexture;
            if (layer == 0)
            {
                int num = (int)((particles[i].TimeOffset * 4f + animTimer) % 4f);
                mtexture = particleTextures[3 - num];
            }
            else if (layer == 1)
            {
                int num2 = (int)((particles[i].TimeOffset * 2f + animTimer) % 2f);
                mtexture = particleTextures[1 + num2];
            }
            else
            {
                mtexture = particleTextures[2];
            }

            if (vector.X >= X + 2f && vector.Y >= Y + 2f && vector.X < Right - 2f && vector.Y < Bottom - 2f)
            {
                mtexture.DrawCentered(vector + shake, color);
            }
        }

        if (whiteFill > 0f)
        {
            Draw.Rect(X + shake.X, Y + shake.Y, Width, Height * whiteHeight, Color.White * whiteFill);
        }


        // Outline, 最后画应该也是为了顺便遮住星星
        CustomWobbleLine(shake + new Vector2(X, Y), shake + new Vector2(X + Width, Y), 0f);
        CustomWobbleLine(shake + new Vector2(X + Width, Y), shake + new Vector2(X + Width, Y + Height), 0.7f);
        CustomWobbleLine(shake + new Vector2(X + Width, Y + Height), shake + new Vector2(X, Y + Height), 1.5f);
        CustomWobbleLine(shake + new Vector2(X, Y + Height), shake + new Vector2(X, Y), 2.5f);

        Draw.Rect(shake + new Vector2(X, Y), 2f, 2f, (playerHasDreamDash ? activeLineColor : disabledLineColor) * OutlineAlpha);
        Draw.Rect(shake + new Vector2(X + Width - 2f, Y), 2f, 2f, (playerHasDreamDash ? activeLineColor : disabledLineColor) * OutlineAlpha);
        Draw.Rect(shake + new Vector2(X, Y + Height - 2f), 2f, 2f, (playerHasDreamDash ? activeLineColor : disabledLineColor) * OutlineAlpha);
        Draw.Rect(shake + new Vector2(X + Width - 2f, Y + Height - 2f), 2f, 2f, (playerHasDreamDash ? activeLineColor : disabledLineColor) * OutlineAlpha);
        activeBackColor = activeBackColorBackup;
        activeLineColor = activeLineColorBackup;
    }

    public void CustomWobbleLine(Vector2 from, Vector2 to, float offset)
    {
        float length = (to - from).Length();
        Vector2 dir = Vector2.Normalize(to - from);
        Vector2 perpendicular = new Vector2(dir.Y, -dir.X);
        Color outlineColor = (playerHasDreamDash ? activeLineColor : disabledLineColor) * OutlineAlpha;
        Color backgroundColor = (playerHasDreamDash ? activeBackColor : disabledBackColor) * BackgroundAlpha;
        if (whiteFill > 0.0)
        {
            outlineColor = Color.Lerp(outlineColor, Color.White, whiteFill);
            backgroundColor = Color.Lerp(backgroundColor, Color.White, whiteFill);
        }

        if (DisableWobble)
        {
            Draw.Line(from, to, outlineColor);
            return;
        }

        float preAmplitude = 0.0f;
        int gap = 16;
        for (int index = 2; index < length - 2.0; index += gap)
        {
            float curAmplitude = Lerp(LineAmplitude(wobbleFrom + offset, index), LineAmplitude(wobbleTo + offset, index), wobbleEase);
            if (index + gap >= (double)length)
                curAmplitude = 0.0f;
            float num4 = Math.Min(gap, length - 2f - index);
            Vector2 start = from + dir * index + perpendicular * preAmplitude;
            Vector2 end = from + dir * (index + num4) + perpendicular * curAmplitude;
            Draw.Line(start - perpendicular, end - perpendicular, backgroundColor);
            Draw.Line(start - perpendicular * 2f, end - perpendicular * 2f, backgroundColor);
            Draw.Line(start, end, outlineColor);
            preAmplitude = curAmplitude;
        }
    }
}