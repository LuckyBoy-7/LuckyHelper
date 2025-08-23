using LuckyHelper.Extensions;
using LuckyHelper.Module;
using LuckyHelper.Modules;
using LuckyHelper.Utils;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;

namespace LuckyHelper.Components;

public class ColorModifierComponent(bool active = true, bool visible = true) : Component(active, visible)
{
    public Color TargetColor => Colors[0];
    public List<Color> Colors;
    private Dictionary<GraphicsComponent, Color> graphicToOrigColor = new();
    private Dictionary<VertexLight, Color> vertexLightToOrigColor = new();

    public static Dictionary<Entity, ColorModifierComponent> EntityToModifier = new();


    public static Color OverrideColor;
    public static bool UseOverrideColor;


    public static BlendState CustomBlurredScreenToMask = new BlendState()
    {
        ColorSourceBlend = Blend.DestinationColor, // src * dest
        ColorDestinationBlend = Blend.Zero, // + 0
        ColorBlendFunction = BlendFunction.Add, // multiply
        AlphaSourceBlend = Blend.Zero,
        AlphaDestinationBlend = Blend.One,
        AlphaBlendFunction = BlendFunction.Add
    };

    public override void Update()
    {
        base.Update();
        foreach (var component in Entity.Components)
        {
            if (component is GraphicsComponent graphics)
            {
                if (!graphicToOrigColor.ContainsKey(graphics))
                    graphicToOrigColor[graphics] = graphics.Color;
                graphics.Color = TargetColor;
            }

            else if (component is VertexLight light)
            {
                if (!vertexLightToOrigColor.ContainsKey(light))
                    vertexLightToOrigColor[light] = light.Color;
                light.Color = Color.Green;
            }
        }
    }

    public override void Removed(Entity entity)
    {
        base.Removed(entity);
        foreach (var component in Entity.Components)
        {
            if (component is GraphicsComponent graphics)
            {
                if (graphicToOrigColor.TryGetValue(graphics, out var color))
                    graphics.Color = color;
            }
            else if (component is VertexLight light)
            {
                if (vertexLightToOrigColor.TryGetValue(light, out var color))
                    light.Color = color;
            }
        }

        EntityToModifier.Remove(entity);
    }

    public override void Added(Entity entity)
    {
        base.Added(entity);
        EntityToModifier[entity] = this;
    }

    public void ApplayColorBegin()
    {
        UseOverrideColor = true;
        OverrideColor = TargetColor;
    }

    public void ApplayColorEnd()
    {
        UseOverrideColor = false;
    }

    private static void EntityListOnRenderExcept(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        if (cursor.TryGotoNext(
                ins => ins.MatchLdloc1(),
                ins => ins.MatchCallvirt(typeof(Entity).GetMethod("Render"))
            ))
        {
            cursor.EmitLdloc1();
            cursor.EmitDelegate<Action<Entity>>((entity) =>
            {
                if (EntityToModifier.TryGetValue(entity, out var modifier))
                {
                    modifier.ApplayColorBegin();
                }
            });
            cursor.Index += 2;
            cursor.EmitLdloc1();
            cursor.EmitDelegate<Action<Entity>>((entity) =>
            {
                if (EntityToModifier.TryGetValue(entity, out var modifier))
                {
                    modifier.ApplayColorEnd();
                }
            });
        }
    }


    private static void BloomRendererOnApply(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        if (cursor.TryGotoNext(ins => ins.MatchCall(typeof(Color).GetProperty("White").GetGetMethod())
            ))

        {
            cursor.Index += 1;
            if (ModCompatModule.FrostHelperLoaded)
                cursor.Index += 1;
            cursor.EmitLdloc(7);
            cursor.EmitDelegate<Func<Color, BloomPoint, Color>>((origColor, bloomPoint) =>
            {
                if (EntityToModifier.TryGetValue(bloomPoint.Entity, out var modifier))
                {
                    return modifier.TargetColor;
                }

                return Color.White; // 不管 frost 怎么做这里都输入白色, 反正原本也是当 mask 用, 这么写没问题
            });
        }

        if (cursor.TryGotoNext(ins => ins.MatchLdsfld(typeof(BloomRenderer).GetField("BlurredScreenToMask"))))
        {
            cursor.Index += 1;
            cursor.EmitDelegate<Func<BlendState, BlendState>>((origBlendState) => { return CustomBlurredScreenToMask; });
        }

        if (cursor.TryGotoNext(ins => ins.MatchCall(typeof(Color).GetProperty("White").GetGetMethod())
            ))

        {
            cursor.Index += 1;
            if (ModCompatModule.FrostHelperLoaded)
                cursor.Index += 1;
            cursor.EmitDelegate<Func<Color, Color>>((origColor) =>
            {
                return Color.White; // 不管 frost 怎么做这里都输入白色, 反正原本也是当 mask 用, 这么写没问题
            });
        }
    }

    private static Color ApplyOverrideColor(Color color)
    {
        if (!UseOverrideColor)
            return color;
        return color.Multiply(OverrideColor);
    }


    [Load]
    public static void Load()
    {
        On.Monocle.Draw.Circle_float_float_float_Color_float_int += DrawOnCircle_float_float_float_Color_float_int;
        On.Monocle.Draw.LineAngle_float_float_float_float_Color += DrawOnLineAngle_float_float_float_float_Color;
        On.Monocle.Draw.LineAngle_Vector2_float_float_Color += DrawOnLineAngle_Vector2_float_float_Color;
        On.Monocle.Draw.LineAngle_Vector2_float_float_Color_float += DrawOnLineAngle_Vector2_float_float_Color_float;
        On.Monocle.Draw.Rect_float_float_float_float_Color += DrawOnRect_float_float_float_float_Color;
        On.Monocle.Draw.HollowRect_float_float_float_float_Color += DrawOnHollowRect_float_float_float_float_Color;
        On.Monocle.Draw.Point += DrawOnPoint;
        IL.Monocle.EntityList.RenderExcept += EntityListOnRenderExcept;
        IL.Celeste.BloomRenderer.Apply += BloomRendererOnApply;
    }


    [Unload]
    public static void Unload()
    {
        On.Monocle.Draw.Circle_float_float_float_Color_float_int -= DrawOnCircle_float_float_float_Color_float_int;
        On.Monocle.Draw.LineAngle_float_float_float_float_Color -= DrawOnLineAngle_float_float_float_float_Color;
        On.Monocle.Draw.LineAngle_Vector2_float_float_Color -= DrawOnLineAngle_Vector2_float_float_Color;
        On.Monocle.Draw.LineAngle_Vector2_float_float_Color_float -= DrawOnLineAngle_Vector2_float_float_Color_float;
        On.Monocle.Draw.Rect_float_float_float_float_Color -= DrawOnRect_float_float_float_float_Color;
        On.Monocle.Draw.HollowRect_float_float_float_float_Color -= DrawOnHollowRect_float_float_float_float_Color;
        On.Monocle.Draw.Point -= DrawOnPoint;
        IL.Monocle.EntityList.RenderExcept -= EntityListOnRenderExcept;
        IL.Celeste.BloomRenderer.Apply -= BloomRendererOnApply;
    }


    private static void DrawOnPoint(On.Monocle.Draw.orig_Point orig, Vector2 at, Color color)
    {
        orig(at, ApplyOverrideColor(color));
    }


    private static void DrawOnLineAngle_Vector2_float_float_Color(On.Monocle.Draw.orig_LineAngle_Vector2_float_float_Color orig, Vector2 start, float angle, float length,
        Color color)
    {
        orig(start, angle, length, ApplyOverrideColor(color));
    }


    private static void DrawOnHollowRect_float_float_float_float_Color(On.Monocle.Draw.orig_HollowRect_float_float_float_float_Color orig, float x, float y, float width,
        float height,
        Color color)
    {
        orig(x, y, width, height, ApplyOverrideColor(color));
    }


    private static void DrawOnRect_float_float_float_float_Color(On.Monocle.Draw.orig_Rect_float_float_float_float_Color orig, float x, float y, float width, float height,
        Color color)
    {
        orig(x, y, width, height, ApplyOverrideColor(color));
    }

    private static void DrawOnLineAngle_Vector2_float_float_Color_float(On.Monocle.Draw.orig_LineAngle_Vector2_float_float_Color_float orig, Vector2 start, float angle,
        float length,
        Color color, float thickness)
    {
        orig(start, angle, length, ApplyOverrideColor(color), thickness);
    }

    private static void DrawOnLineAngle_float_float_float_float_Color(On.Monocle.Draw.orig_LineAngle_float_float_float_float_Color orig, float startX, float startY, float angle,
        float length,
        Color color)
    {
        orig(startX, startY, angle, length, ApplyOverrideColor(color));
    }

    private static void DrawOnCircle_float_float_float_Color_float_int(On.Monocle.Draw.orig_Circle_float_float_float_Color_float_int orig, float x, float y, float radius,
        Color color,
        float thickness, int resolution)
    {
        orig(x, y, radius, ApplyOverrideColor(color), thickness, resolution);
    }
}