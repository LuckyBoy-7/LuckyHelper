using Celeste.Mod.Entities;

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

    public string StarColors1 = "FFEF11,FF00D0,08a310";
    public string StarColors2 = "5fcde4,7fb25e,E0564C";
    public string StarColors3 = "5b6ee1,CC3B3B,7daa64";

    public bool DisableWobble = true;
    public bool DisableInteraction = false;
    public bool DisableCollisionOnNotDreaming = false;
    public bool CancelDreamDashOnNotDreaming = false;

    public bool OldVersionThatHasCollisionWithDisabledDreamZone;
    public double StarNumberPerUnit;
    public float StarAlpha;

    public DreamZone(EntityData data, Vector2 offset) : base(data, offset)
    {
        StopPlayerOnCollide = data.Bool("stopPlayerOnCollide");
        KillPlayerOnCollide = data.Bool("killPlayerOnCollide");

        BackgroundColor = data.HexColor("backgroundColor");
        BackgroundAlpha = data.Float("backgroundAlpha");
        OutlineColor = data.HexColor("outlineColor");
        OutlineAlpha = data.Float("outlineAlpha");

        StarColors1 = data.Attr("starColors1");
        StarColors2 = data.Attr("starColors2");
        StarColors3 = data.Attr("starColors3");

        if (data.Has("startColors1"))
            StarColors1 = data.Attr("startColors1");
        if (data.Has("startColors2"))
            StarColors2 = data.Attr("startColors2");
        if (data.Has("startColors3"))
            StarColors3 = data.Attr("startColors3");

        DisableWobble = data.Bool("disableWobble");
        DisableInteraction = data.Bool("disableInteraction");
        DisableCollisionOnNotDreaming = data.Bool("disableCollisionOnNotDreaming");
        // 打错字惹
        if (data.Has("disableCollisioinOnNotDreaming"))
            DisableCollisionOnNotDreaming = data.Bool("disableCollisioinOnNotDreaming");
        OldVersionThatHasCollisionWithDisabledDreamZone = !data.Has("disableCollisionOnNotDreaming") || data.Bool("useOldFeatureOnNotDreaming");
        CancelDreamDashOnNotDreaming = data.Bool("cancelDreamDashOnNotDreaming");
        StarNumberPerUnit = data.Float("starNumberPerUnit", 0.7f);
        StarAlpha = data.Float("starAlpha", 1); 

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

                    particles[i].Color = GetRandomColorByString(StarColors1);
                    break;
                case 1:
                    particles[i].Color = GetRandomColorByString(StarColors2);
                    break;
                case 2:
                    particles[i].Color = GetRandomColorByString(StarColors3);
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
        if (BackgroundAlpha == 1 && !DisableWobble)
            Draw.Rect(shake.X + X, shake.Y + Y, Width, Height, (playerHasDreamDash ? activeBackColor : disabledBackColor) * BackgroundAlpha);
        else
            Draw.Rect(shake.X + X + 1, shake.Y + 1 + Y, Width - 2, Height - 2, (playerHasDreamDash ? activeBackColor : disabledBackColor) * BackgroundAlpha);


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
                mtexture.DrawCentered(vector + shake, color * StarAlpha);
            }
        }

        if (whiteFill > 0f)
        {
            Draw.Rect(X + shake.X, Y + shake.Y, Width, Height * whiteHeight, Color.White * whiteFill);
        }


        // Outline, 最后画应该也是为了顺便遮住星星
        // Draw.Point(Position, Color.Yellow);
        // Draw.Point(new Vector2(X + Width, Y), Color.Yellow);
        // Draw.Line(new Vector2(X + Width, Y),new Vector2(X + Width, Y + 4), Color.Yellow);
        // Draw.Line(Position-Vector2.UnitY, Position + Vector2.UnitX * 3 -Vector2.UnitY, Color.Yellow);
        // Draw.Line(Position, Position -Vector2.UnitY*3, Color.Yellow);
        // Draw.Line(Position - Vector2.UnitY, Position + Vector2.UnitX * 3 - Vector2.UnitY * 3, Color.Yellow);
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

        // // test
        // Draw.Line(from + perpendicular*0.49f, to - dir*0.49f+perpendicular * 0.6f, Color.Red);
        // Draw.Line(from, to, outlineColor);
        // return;

        // Draw.Line(Position, TopRight, Color.Yellow);
        if (DisableWobble)
        {
            Draw.Line(from, to, outlineColor);
            return;
        }


        // draw.line 结束的位置不会画点
        // Draw.Line(Position, Position - Vector2.UnitY + Vector2.UnitX *3, Color.Yellow);
        float preAmplitude = 0.0f;
        int gap = 16;

        if (BackgroundAlpha == 1)
        {
            for (int index = 2; index < length - 2.0; index += gap)
            {
                float curAmplitude = Lerp(LineAmplitude(wobbleFrom + offset, index), LineAmplitude(wobbleTo + offset, index), wobbleEase);
                if (index + gap >= (double)length)
                    curAmplitude = 0.0f;
                float segmentLength = Math.Min(gap, length - 2f - index);
                Vector2 startPos = from + dir * index + perpendicular * preAmplitude;
                Vector2 endPos = from + dir * (index + segmentLength) + perpendicular * curAmplitude;

                Draw.Line(startPos - perpendicular, endPos - perpendicular, backgroundColor);
                Draw.Line(startPos - perpendicular * 2f, endPos - perpendicular * 2f, backgroundColor);
                Draw.Line(startPos, endPos, outlineColor);
                preAmplitude = curAmplitude;
            }

            return;
        }

        preAmplitude = 0;
        for (int index = 2; index < length - 2.0; index += gap)
        {
            float curAmplitude = Lerp(LineAmplitude(wobbleFrom + offset, index), LineAmplitude(wobbleTo + offset, index), wobbleEase);
            if (index + gap >= (double)length)
                curAmplitude = 0.0f;
            float segmentLength = Math.Min(gap, length - 2f - index);
            Vector2 startPos = from + dir * index + perpendicular * preAmplitude;
            Vector2 endPos = from + dir * (index + segmentLength) + perpendicular * curAmplitude;
            for (int i = 0; i < segmentLength; i++)
            {
                // float a = Lerp(LineAmplitude(wobbleFrom + offset, index + i), LineAmplitude(wobbleTo + offset, index + i), wobbleEase);
                Vector2 s = from + dir * (index + i) - perpendicular;
                Vector2 e = Vector2.Lerp(startPos, endPos, i / (segmentLength)) - perpendicular;

                Draw.Line(s, e, backgroundColor);
            }

            preAmplitude = curAmplitude;
        }


        preAmplitude = 0.0f;
        for (int index = 2; index < length - 2.0; index += gap)
        {
            float curAmplitude = Lerp(LineAmplitude(wobbleFrom + offset, index), LineAmplitude(wobbleTo + offset, index), wobbleEase);
            if (index + gap >= (double)length)
                curAmplitude = 0.0f;
            float segmentLength = Math.Min(gap, length - 2f - index);
            Vector2 startPos = from + dir * index + perpendicular * preAmplitude;
            Vector2 endPos = from + dir * (index + segmentLength) + perpendicular * curAmplitude;

            // Draw.Line(start - perpendicular, end - perpendicular, backgroundColor);
            // Draw.Line(start - perpendicular * 2f, end - perpendicular * 2f, backgroundColor);
            Draw.Line(startPos, endPos, outlineColor);
            preAmplitude = curAmplitude;
        }
    }
}