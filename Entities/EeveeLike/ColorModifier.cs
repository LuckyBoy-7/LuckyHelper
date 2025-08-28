using Celeste.Mod.Entities;
using LuckyHelper.Components;
using LuckyHelper.Components.EeveeLike;
using LuckyHelper.Extensions;
using LuckyHelper.Utils;


namespace LuckyHelper.Entities.EeveeLike;

public enum ColorSourceMode
{
    FirstColor,
    CycleColor,
    RandomColor,
    Rainbow,
    ByFlags,
}

public enum ColorTransitionMode
{
    Lerp,
    Blink,
}

public enum ColorBlendMode
{
    Multiply,
    Replace
}

public class ColorController : Component
{
    public Color CurrentColor(Vector2 position)
    {
        switch (ColorSourceMode)
        {
            case ColorSourceMode.FirstColor:
                return Colors[0];
            case ColorSourceMode.CycleColor:
            case ColorSourceMode.RandomColor:
            case ColorSourceMode.ByFlags:
                return CurrentChangingColor;
            case ColorSourceMode.Rainbow:
                return GetHue(position);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public ColorSourceMode ColorSourceMode;
    public ColorTransitionMode ColorTransitionMode;
    public List<Color> Colors;
    public List<string> Flags;
    public Entity Parent;


    public ColorController(List<Color> colors) : base(true, false)
    {
        Colors = colors;
        CurrentChangingColor = colors[0];
    }

    // From Color Sequence
    public float ColorChangeSpeed = 1f;
    public Color CurrentChangingColor;

    public Color NextColor => Colors[NextColorIndex];

    //
    public int NextColorIndex = 0;

    public float blinkTime => ColorChangeSpeed != 0 ? 100 / ColorChangeSpeed : float.MaxValue;
    public float blinkElapse;


    public override void Update()
    {
        base.Update();
        if (ColorSourceMode is ColorSourceMode.CycleColor or ColorSourceMode.RandomColor or ColorSourceMode.ByFlags)
        {
            if (ColorTransitionMode is ColorTransitionMode.Lerp)
            {
                CurrentChangingColor = CurrentChangingColor.Approach(NextColor, ColorChangeSpeed * Engine.DeltaTime);
            }
            else if (ColorTransitionMode is ColorTransitionMode.Blink)
            {
                blinkElapse += Engine.DeltaTime;
                if (blinkElapse >= blinkTime)
                {
                    blinkElapse -= blinkTime;
                    CurrentChangingColor = NextColor;
                }
            }

            if (ColorSourceMode is ColorSourceMode.ByFlags)
            {
                for (var i = 0; i < Flags.Count; i++)
                {
                    var flag = Flags[i];
                    if (Parent.Session().GetFlag(flag))
                    {
                        NextColorIndex = Math.Min(i, Colors.Count - 1);
                        return;
                    }
                }
            }

            if (CurrentChangingColor == NextColor)
            {
                if (ColorSourceMode is ColorSourceMode.CycleColor)
                {
                    NextColorIndex = (NextColorIndex + 1) % Colors.Count;
                }
                else if (ColorSourceMode is ColorSourceMode.RandomColor)
                {
                    NextColorIndex = Calc.Random.Range(0, Colors.Count);
                }
            }
        }
    }

    private Color GetHue(Vector2 position)
    {
        float num = 280f;
        float num2 = (position.Length() + Engine.Scene.TimeActive * ColorChangeSpeed / 2) % num / num;
        return Calc.HsvToColor(0.4f + Calc.YoYo(num2) * 0.4f, 0.4f, 0.9f);
    }
}

[Tracked]
[CustomEntity("LuckyHelper/ColorModifier")]
public class ColorModifier : Actor, IContainer
{
    public EntityContainer Container { get; }
    // public List<Color> Colors;

    public ColorController ColorController;

    public ColorModifier(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        Collider = new Hitbox(data.Width, data.Height);

        Depth = Depths.Top - 10;


        Add(ColorController = new ColorController(data.ParseToColorList("colors"))
        {
            ColorSourceMode = data.Enum<ColorSourceMode>("colorSourceMode"),
            ColorTransitionMode = data.Enum<ColorTransitionMode>("colorTransitionMode"),
            ColorChangeSpeed = data.Float("colorChangeSpeed"),
            Flags = data.ParseToStringList("flags"),
            Parent = this
        });

        Add(Container = new EntityContainer(data)
        {
            DefaultIgnored = e => e is ColorModifier,
            OnAttach = handler =>
            {
                handler.Entity.AddNoDuplicatedComponent(new ColorModifierComponent()
                {
                    AffectTexture = data.Bool("affectTexture", true),
                    AffectLight = data.Bool("affectLight", true),
                    AffectGeometry = data.Bool("affectGeometry", true),
                    AffectParticle = data.Bool("affectParticle", true),
                    GetCurrentColor = (position) => ColorController.CurrentColor(position),
                    EntityHandler = handler,
                    ColorBlendMode = data.Enum<ColorBlendMode>("colorBlendMode")
                });
            },
            OnDetach = handler => { handler.Entity.Get<ColorModifierComponent>()?.RemoveSelf(); }
        });
    }
}