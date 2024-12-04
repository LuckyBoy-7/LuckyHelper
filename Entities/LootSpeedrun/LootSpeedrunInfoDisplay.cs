using Celeste.Mod.Entities;
using LuckyHelper.Extensions;

namespace LuckyHelper.Entities.LootSpeedrun;

[CustomEntity("LuckyHelper/LootSpeedrunInfoDisplay")]
[Tracked]
public class LootSpeedrunInfoDisplay : Entity
{
    private static float numberWidth;
    private static float spacerWidth;
    private MTexture bg = GFX.Gui["strawberryCountBG"];
    public float DrawLerp;
    private float target;

    private float curValue;
    // -------------------------------------

    public float Time;
    public int Value;

    public LootSpeedrunInfoDisplay()
    {
        Tag = Tags.HUD | Tags.Global;
        Depth = -100;
        Y = 60f;
        CalculateBaseSizes();
    }

    public static void CalculateBaseSizes()
    {
        PixelFont font = Dialog.Languages["english"].Font;
        float fontFaceSize = Dialog.Languages["english"].FontFaceSize;
        PixelFontSize pixelFontSize = font.Get(fontFaceSize);
        for (int i = 0; i < 10; i++)
        {
            float x = pixelFontSize.Measure(i.ToString()).X;
            if (x > numberWidth)
            {
                numberWidth = x;
            }
        }

        spacerWidth = pixelFontSize.Measure('.').X;
    }

    public override void Update()
    {
        DrawLerp = Calc.Approach(DrawLerp, target, Engine.DeltaTime * 4f);
        if (Math.Abs(curValue - Value) > 5)
            curValue = Calc.LerpClamp(curValue, Value, 0.07f);
        else
            curValue = Calc.Approach(curValue, Value, 2);

        // 如果当前房间不是劫掠房, 则显示历史最高分
        if (this.GetEntity<LootSpeedrunController>() == null)
            Value = this.Session().GetCounter(LootSpeedrunController.LootMaxValueID);


        base.Update();
    }

    public override void Render()
    {
        if (DrawLerp <= 0f)
        {
            return;
        }

        float x = 300f * Ease.CubeIn(1f - DrawLerp);
        x += 1920;


        // 所剩时间
        string timeText = Time.ToString("F3");
        float timeWidth = GetTimeWidth(timeText);
        Vector2 timePos = new Vector2(x - 32 - timeWidth, Y);
        bg.Draw(timePos + new Vector2(-20, 0), new Vector2(bg.Width, 0), Color.Black, new Vector2(-1, 1));
        Draw.Rect(timePos + new Vector2(40, 0), 1920, 38, Color.Black);
        DrawTime(timePos + new Vector2(0, 44), timeText);

        // 积分
        string valueString = ((int)curValue).ToString();
        Vector2 valuePos = new Vector2(x - 32 - GetTimeWidth(valueString, 0.6f), Y + 40f + 26.400002f);
        bg.Draw(valuePos + new Vector2(-15, -28), new Vector2(bg.Width, 0), Color.Black, new Vector2(-1, 1) * 0.6f);
        Draw.Rect(valuePos + new Vector2(0, -28), 1920, 38 * 0.6f, Color.Black);
        DrawTime(valuePos, valueString, 0.6f, true, false, false, 0.6f);
    }

    public void Show() => target = 1;
    public void Hide() => target = 0;

    public static void DrawTime(Vector2 position, string timeString, float scale = 1f, bool valid = true, bool finished = false, bool bestTime = false,
        float alpha = 1f)
    {
        PixelFont font = Dialog.Languages["english"].Font;
        float fontFaceSize = Dialog.Languages["english"].FontFaceSize;
        float num = scale;
        float num2 = position.X;
        float num3 = position.Y;
        Color color = Color.White * alpha;
        Color color2 = Color.LightGray * alpha;
        if (!valid)
        {
            color = Calc.HexToColor("918988") * alpha;
            color2 = Calc.HexToColor("7a6f6d") * alpha;
        }
        else if (bestTime)
        {
            color = Calc.HexToColor("fad768") * alpha;
            color2 = Calc.HexToColor("cfa727") * alpha;
        }
        else if (finished)
        {
            color = Calc.HexToColor("6ded87") * alpha;
            color2 = Calc.HexToColor("43d14c") * alpha;
        }

        foreach (char c in timeString)
        {
            if (c == '.')
            {
                num = scale * 0.7f;
                num3 -= 5f * scale;
            }

            Color color3 = ((c == ':' || c == '.' || num < scale) ? color2 : color);
            float num4 = (((c == ':' || c == '.') ? spacerWidth : numberWidth) + 4f) * num;
            font.DrawOutline(
                fontFaceSize, c.ToString(), new Vector2(num2 + num4 / 2f, num3), new Vector2(0.5f, 1f), Vector2.One * num, color3, 2f, Color.Black
            );
            num2 += num4;
        }
    }

    public static float GetTimeWidth(string timeString, float scale = 1f)
    {
        float num = scale;
        float num2 = 0f;
        foreach (char c in timeString)
        {
            if (c == '.')
            {
                num = scale * 0.7f;
            }

            float num3 = (((c == ':' || c == '.') ? spacerWidth : numberWidth) + 4f) * num;
            num2 += num3;
        }

        return num2;
    }

}