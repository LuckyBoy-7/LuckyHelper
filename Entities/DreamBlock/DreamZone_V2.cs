using Celeste.Mod.Entities;
using LuckyHelper.Extensions;
using LuckyHelper.Module;
using LuckyHelper.Modules;
using MonoMod.Cil;

namespace LuckyHelper.Entities;

[CustomEntity("LuckyHelper/DreamZone_V2")]
[TrackedAs(typeof(DreamBlock))]
[Tracked]
public class DreamZone_V2 : DreamBlock
{
    public bool StopPlayerOnCollide = true;
    public bool KillPlayerOnCollide = true;

    public Color ActiveBackgroundColor = Color.Green;
    public Color DisabledBackgroundColor = Color.Green;

    public Color ActiveLineColor = Color.Black;
    public Color DisabledLineColor = Color.Black;

    public float ActiveBackgroundAlpha = 1f;
    public float DisabledBackgroundAlpha = 1f;


    public float ActiveLineAlpha = 0.5f;
    public float DisabledLineAlpha = 0.5f;

    public string BigStarColors = "FFEF11,FF00D0,08a310";
    public string MediumStarColors = "5fcde4,7fb25e,E0564C";
    public string SmallStarColors = "5b6ee1,CC3B3B,7daa64";

    public bool DisableWobble = true;
    public bool DisableInteraction = false;
    public bool CancelDreamDashOnNotDreaming = false;

    public double StarNumberPerUnit;
    public float ActiveStarAlpha;
    public float DisabledStarAlpha;
    public bool DisableVerticalJump;
    public bool DisableInsideDreamJump;
    public bool GetVerticalCoyote;
    public bool ConserveSpeed;
    public int DashesToRefill;


    public DreamZone_V2(EntityData data, Vector2 offset) : base(data, offset)
    {
        StopPlayerOnCollide = data.Bool("stopPlayerOnCollide");
        KillPlayerOnCollide = data.Bool("killPlayerOnCollide");

        ActiveBackgroundColor = data.FitColor(Color.Black, "activeBackgroundColor", "backgroundColor");
        DisabledBackgroundColor = data.FitColor(Calc.HexToColor("1f2e2d"), "disabledBackgroundColor");

        ActiveLineColor = data.FitColor(Color.White, "activeLineColor", "outlineColor");
        DisabledLineColor = data.FitColor(Calc.HexToColor("6a8480"), "disabledLineColor");

        ActiveBackgroundAlpha = data.FitFloat(0, "activeBackgroundAlpha", "backgroundAlpha");
        DisabledBackgroundAlpha = data.FitFloat(0, "disabledBackgroundAlpha", "backgroundAlpha");


        ActiveLineAlpha = data.FitFloat(0, "activeLineAlpha", "outlineAlpha");
        DisabledLineAlpha = data.FitFloat(0, "disabledLineAlpha", "outlineAlpha");


        BigStarColors = data.Attr("bigStarColors");
        MediumStarColors = data.Attr("mediumStarColors");
        SmallStarColors = data.Attr("smallStarColors");

        DisableWobble = data.Bool("disableWobble");
        DisableInteraction = data.Bool("disableInteraction");
        CancelDreamDashOnNotDreaming = data.Bool("cancelDreamDashOnNotDreaming");
        StarNumberPerUnit = data.Float("starNumberPerUnit", 0.7f);
        if (data.Has("starAlpha")) // 老版
        {
            DisabledStarAlpha = ActiveStarAlpha = data.Float("starAlpha");
        }
        else
        {
            ActiveStarAlpha = data.Float("activeStarAlpha");
            DisabledStarAlpha = data.Float("disabledStarAlpha");
        }

        DisableVerticalJump = data.Bool("disableVerticalJump", false);
        DisableInsideDreamJump = data.Bool("disableInsideDreamJump", false);
        GetVerticalCoyote = data.Bool("getVerticalCoyote", false);
        ConserveSpeed = data.Bool("conserveSpeed", false);
        DashesToRefill = data.Int("dashesToRefill", 1);

        Collidable = false;
    }

    public override void Update()
    {
        base.Update();
        if (occlude != null)
        {
            occlude.RemoveSelf();
            occlude = null;
        }
    }


    public Color GetRandomColorByString(string str)
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
        Color disabledBackColorBackup = disabledBackColor;
        Color disabledLineColorBackup = disabledLineColor;
        activeBackColor = ActiveBackgroundColor;
        activeLineColor = ActiveLineColor;
        disabledBackColor = DisabledBackgroundColor;
        disabledLineColor = DisabledLineColor;
        if ((ActiveBackgroundAlpha == 1 || DisabledBackgroundAlpha == 1) && !DisableWobble)
            Draw.Rect(shake.X + X, shake.Y + Y, Width, Height, (playerHasDreamDash ? activeBackColor * ActiveBackgroundAlpha : disabledBackColor * DisabledBackgroundAlpha));
        else
            Draw.Rect(shake.X + X + 1, shake.Y + 1 + Y, Width - 2, Height - 2,
                (playerHasDreamDash ? activeBackColor * ActiveBackgroundAlpha : disabledBackColor * DisabledBackgroundAlpha));


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
                mtexture.DrawCentered(vector + shake, color * (playerHasDreamDash ? ActiveStarAlpha : DisabledStarAlpha));
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

        Draw.Rect(shake + new Vector2(X, Y), 2f, 2f, (playerHasDreamDash ? activeLineColor * ActiveLineAlpha : disabledLineColor * DisabledLineAlpha));
        Draw.Rect(shake + new Vector2(X + Width - 2f, Y), 2f, 2f, (playerHasDreamDash ? activeLineColor * ActiveLineAlpha : disabledLineColor * DisabledLineAlpha));
        Draw.Rect(shake + new Vector2(X, Y + Height - 2f), 2f, 2f, (playerHasDreamDash ? activeLineColor * ActiveLineAlpha : disabledLineColor * DisabledLineAlpha));
        Draw.Rect(shake + new Vector2(X + Width - 2f, Y + Height - 2f), 2f, 2f, (playerHasDreamDash ? activeLineColor * ActiveLineAlpha : disabledLineColor * DisabledLineAlpha));
        activeBackColor = activeBackColorBackup;
        activeLineColor = activeLineColorBackup;
        disabledBackColor = disabledBackColorBackup;
        disabledLineColor = disabledLineColorBackup;
    }

    public void CustomWobbleLine(Vector2 from, Vector2 to, float offset)
    {
        float length = (to - from).Length();
        Vector2 dir = Vector2.Normalize(to - from);
        Vector2 perpendicular = new Vector2(dir.Y, -dir.X);
        Color outlineColor = (playerHasDreamDash ? activeLineColor * ActiveLineAlpha : disabledLineColor * DisabledLineAlpha);
        Color backgroundColor = (playerHasDreamDash ? activeBackColor * ActiveBackgroundAlpha : disabledBackColor * DisabledBackgroundAlpha);
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

        if (ActiveBackgroundAlpha == 1 || DisabledBackgroundAlpha == 1)
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

    [Load]
    public static void Load()
    {
        IL.Celeste.Player.DreamDashBegin += PlayerOnDreamDashBegin;
        IL.Celeste.Player.DreamDashEnd += PlayerOnDreamDashEnd;
    }


    [Unload]
    public static void Unload()
    {
        IL.Celeste.Player.DreamDashBegin -= PlayerOnDreamDashBegin;
        IL.Celeste.Player.DreamDashEnd -= PlayerOnDreamDashEnd;
    }

    private static void PlayerOnDreamDashEnd(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);

        if (cursor.TryGotoNext(ins => ins.MatchCallvirt(typeof(Player).GetMethod("RefillDash"))
            ))
        {
            cursor.Index += 1;
            cursor.EmitLdarg0();
            cursor.EmitDelegate<Action<Player>>(player =>
            {
                if (DreamZone_V2Module.DreamZone is { } dreamZone)
                {
                    if (player.Dashes < dreamZone.DashesToRefill)
                    {
                        player.Dashes = dreamZone.DashesToRefill;
                    }
                }
            });
        }
    }

    private static void PlayerOnDreamDashBegin(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);

        if (cursor.TryGotoNext(ins => ins.MatchLdcR4(240)
            ))
        {
            cursor.Index += 1;
            cursor.EmitLdarg0();
            cursor.EmitDelegate<Func<float, Player, float>>((origSpeed, player) =>
            {
                if (DreamZone_V2Module.DreamZone is { } dreamZone)
                {
                    if (dreamZone.ConserveSpeed)
                    {
                        return player.Speed.Length();
                    }
                }

                return origSpeed;
            });
        }
    }
}