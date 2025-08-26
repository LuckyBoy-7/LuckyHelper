using System.Net.Http.Headers;
using System.Reflection;
using LuckyHelper.Entities.EeveeLike;
using LuckyHelper.Extensions;
using LuckyHelper.Handlers;
using LuckyHelper.Module;
using LuckyHelper.Modules;
using LuckyHelper.Utils;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using EntityList = On.Monocle.EntityList;
using Level = On.Celeste.Level;
using Particle = On.Monocle.Particle;

namespace LuckyHelper.Components;

public class ColorModifierComponent(bool active = true, bool visible = true) : Component(active, visible)
{
    public Func<Vector2, Color> GetCurrentColor;

    public Color GetHandledColor(Color color)
    {
        switch (ColorBlendMode)
        {
            case ColorBlendMode.Multiply:
                return color.Multiply(GetCurrentColor(Entity.Position));
            case ColorBlendMode.Replace:
                return GetCurrentColor(Entity.Position);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    // 管 Sprite.Color Image.Color 的
    private Dictionary<GraphicsComponent, Color> graphicToOrigColor = new();

    // 管 VertexLight.Color 的
    private Dictionary<VertexLight, Color> vertexLightToOrigColor = new();

    // 快速查询 entity 对应的 modifier
    public static Dictionary<Entity, ColorModifierComponent> EntityToModifier = new();

    // 管 Draw.Rect Draw.Circle 之类的
    public static Color OverrideGeometryParticleColor;
    public static bool UseOverrideGeometryColor;

    // 在 render 前暂时修改 Color.White, 因为很多画贴图的函数都是用 Color 或者直接用 White 的, 所以其实能覆盖挺多情况的
    public static DynamicData WhiteDyn = new DynamicData(Color.White);
    public static Color OrigWhiteColor = Color.White;

    // 管各种 entity.Color 字段的
    public DynamicData EntityDyn;
    public Dictionary<string, Color> EntityFieldToOrigColor = new();

    public IEntityHandler EntityHandler;

    public static BlendState CustomBlurredScreenToMask = new BlendState()
    {
        ColorSourceBlend = Blend.DestinationColor, // src * dest
        ColorDestinationBlend = Blend.Zero, // + 0
        ColorBlendFunction = BlendFunction.Add, // multiply
        AlphaSourceBlend = Blend.Zero,
        AlphaDestinationBlend = Blend.One,
        AlphaBlendFunction = BlendFunction.Add
    };

    public bool AffectTexture;

    public bool AffectLight;
    // public bool AffectBloom;  // 大多数时候可以理解成一个东西

    public bool AffectGeometry;
    public bool AffectParticle;
    public ColorBlendMode ColorBlendMode;
    public static ColorBlendMode CommonColorBlendMode;


    public override void Removed(Entity entity)
    {
        base.Removed(entity);
        EntityToModifier.Remove(entity);
    }

    public override void EntityRemoved(Scene scene)
    {
        base.EntityRemoved(scene);
        EntityToModifier.Remove(Entity);
    }

    public override void Added(Entity entity)
    {
        base.Added(entity);
        EntityToModifier[entity] = this;
        EntityDyn = new(entity);

        foreach (var component in entity.Components)
        {
            if (component is GraphicsComponent graphics)
            {
                graphicToOrigColor[graphics] = Color.White;
            }
            else if (component is VertexLight light)
            {
                vertexLightToOrigColor[light] = Color.White;
            }
        }
    }

    public void BeforeRender(bool affectFields = true)
    {
        CommonColorBlendMode = ColorBlendMode;
        OverrideGeometryParticleColor = GetCurrentColor(Entity.Position);
        if (AffectGeometry)
        {
            UseOverrideGeometryColor = true;
        }

        if (AffectTexture)
            foreach (GraphicsComponent graphics in graphicToOrigColor.Keys)
            {
                graphicToOrigColor[graphics] = graphics.Color;
                graphics.Color = GetCurrentColor(Entity.Position);
            }

        if (AffectLight)
            foreach (VertexLight light in vertexLightToOrigColor.Keys)
            {
                vertexLightToOrigColor[light] = light.Color;
                light.Color = GetCurrentColor(Entity.Position);
            }


        if (AffectTexture)
        {
            if (!affectFields)
                return;
            // 更多的是影响画默认贴图的
            WhiteDyn.Set("White", GetCurrentColor(Entity.Position));

            foreach (var field in EntityHandler.GetPossibleColorFields())
            {
                // 有这个字段并且还没存过初始值
                // 这里要拿 obj 然后转 Color, 要是直接拿 out Color 的话 cast 会出错 
                if (EntityDyn.TryGet(field, out var value))
                {
                    // 有的字段叫 color 但不是 color 类型, 比如 spinner 的 color

                    if (value is Color origColor)
                    {
                        EntityFieldToOrigColor[field] = origColor;
                        EntityDyn.Set(field, EntityFieldToOrigColor[field].Multiply(GetCurrentColor(Entity.Position)));
                    }
                }
            }
        }
    }

    public void AfterRender(bool affectFields = true)
    {
        if (AffectGeometry)
            UseOverrideGeometryColor = false;
        if (AffectTexture)
            foreach (GraphicsComponent graphics in graphicToOrigColor.Keys)
            {
                graphics.Color = graphicToOrigColor[graphics];
            }

        if (AffectLight)
            foreach (VertexLight light in vertexLightToOrigColor.Keys)
            {
                light.Color = vertexLightToOrigColor[light];
            }

        if (AffectTexture)
        {
            if (!affectFields)
                return;
            WhiteDyn.Set("White", OrigWhiteColor);
            foreach (var field in EntityHandler.GetPossibleColorFields())
            {
                if (EntityDyn.TryGet(field, out var value))
                {
                    // 有的字段叫 color 但不是 color 类型, 比如 spinner 的 color
                    if (value is Color _)
                    {
                        EntityDyn.Set(field, EntityFieldToOrigColor[field]);
                    }
                }
            }
        }
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
                    if (modifier.EntityHandler is IModifyColor iModifyColor)
                    {
                        modifier.BeforeRender(false);
                        iModifyColor.BeforeRender(modifier);
                    }
                    else
                    {
                        modifier.BeforeRender(true);
                    }
                }
            });
            cursor.Index += 2;
            cursor.EmitLdloc1();
            cursor.EmitDelegate<Action<Entity>>((entity) =>
            {
                if (EntityToModifier.TryGetValue(entity, out var modifier))
                {
                    if (modifier.EntityHandler is IModifyColor iModifyColor)
                    {
                        modifier.AfterRender(false);
                        iModifyColor.AfterRender(modifier);
                    }
                    else
                    {
                        modifier.AfterRender(true);
                    }
                }
            });
        }
    }


    private static void BloomRendererOnApply(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        var colorGetWhiteMethod = typeof(Color).GetProperty("White").GetGetMethod();
        if (cursor.TryGotoNext(ins => ins.MatchCall(colorGetWhiteMethod)
            ))

        {
            cursor.Index += 1;
            if (ModCompatModule.FrostHelperLoaded)
                cursor.Index += 1;
            cursor.EmitLdloc(7);
            cursor.EmitDelegate<Func<Color, BloomPoint, Color>>((origColor, bloomPoint) =>
            {
                if (EntityToModifier.TryGetValue(bloomPoint.Entity, out var modifier) && modifier.AffectLight)
                {
                    return modifier.GetCurrentColor(modifier.Entity.Position);
                }

                return Color.White; // 不管 frost 怎么做这里都输入白色, 反正原本也是当 mask 用, 这么写没问题
            });
        }

        if (cursor.TryGotoNext(ins => ins.MatchLdsfld(typeof(BloomRenderer).GetField("BlurredScreenToMask"))))
        {
            cursor.Index += 1;
            cursor.EmitDelegate<Func<BlendState, BlendState>>((origBlendState) => { return CustomBlurredScreenToMask; });
        }
    }

    private static void TileGridOnRenderAt(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        if (cursor.TryGotoNext(ins => ins.MatchLdloc(4)
            ))

        {
            cursor.Index += 1;
            cursor.EmitLdarg0();
            cursor.EmitDelegate<Func<Color, TileGrid, Color>>((origColor, tileGrid) =>
            {
                // 找了半天 bug 发现好像所有蔚蓝房间共用一个 TileGrid
                if (EntityToModifier.TryGetValue(tileGrid.Entity, out var modifier) && modifier.AffectTexture)
                {
                    return modifier.GetCurrentColor(modifier.Entity.Position);
                }

                return origColor;
            });
        }
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
        IL.Monocle.TileGrid.RenderAt += TileGridOnRenderAt;
        On.Celeste.Level.LoadLevel += LevelOnLoadLevel;
        On.Monocle.Entity.Added += EntityOnAdded;
        On.Monocle.ParticleSystem.Emit_ParticleType_Vector2 += ParticleSystemOnEmit_ParticleType_Vector2;
        On.Monocle.ParticleSystem.Emit_ParticleType_Vector2_Color += ParticleSystemOnEmit_ParticleType_Vector2_Color;
        On.Monocle.ParticleSystem.Emit_ParticleType_Vector2_Color_float += ParticleSystemOnEmit_ParticleType_Vector2_Color_float;
        On.Monocle.ParticleSystem.Emit_ParticleType_Vector2_float += ParticleSystemOnEmit_ParticleType_Vector2_float;
        IL.Monocle.ParticleSystem.Render += ParticleSystemOnRender;
        IL.Monocle.ParticleSystem.Render_float += ParticleSystemOnRender_float;
        On.Monocle.ParticleSystem.Update += ParticleSystemOnUpdate;
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
        IL.Monocle.TileGrid.RenderAt -= TileGridOnRenderAt;

        // 清理 EntityToModifier
        On.Celeste.Level.LoadLevel -= LevelOnLoadLevel;

        // 为了给 particle 绑定 entity 信息的
        On.Monocle.Entity.Added -= EntityOnAdded;
        On.Monocle.ParticleSystem.Emit_ParticleType_Vector2 -= ParticleSystemOnEmit_ParticleType_Vector2;
        On.Monocle.ParticleSystem.Emit_ParticleType_Vector2_Color -= ParticleSystemOnEmit_ParticleType_Vector2_Color;
        On.Monocle.ParticleSystem.Emit_ParticleType_Vector2_Color_float -= ParticleSystemOnEmit_ParticleType_Vector2_Color_float;
        On.Monocle.ParticleSystem.Emit_ParticleType_Vector2_float -= ParticleSystemOnEmit_ParticleType_Vector2_float;

        // 管 particle 颜色的
        IL.Monocle.ParticleSystem.Render -= ParticleSystemOnRender;
        IL.Monocle.ParticleSystem.Render_float -= ParticleSystemOnRender_float;
        On.Monocle.ParticleSystem.Update -= ParticleSystemOnUpdate;
    }


    private static void LevelOnLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Celeste.Level self, Player.IntroTypes playerIntro, bool isFromLoader)
    {
        var tmp = new Dictionary<Entity, ColorModifierComponent>();
        foreach ((Entity entity, ColorModifierComponent modifier) in EntityToModifier)
        {
            if (entity.Scene != null)
            {
                tmp[entity] = modifier;
            }
        }

        EntityToModifier = tmp;
        orig(self, playerIntro, isFromLoader);
    }

    #region Particle

    private static void ParticleSystemOnRender(ILContext il)
    {
        HookParticleSystemRender(il, false);
    }

    private static void ParticleSystemOnRender_float(ILContext il)
    {
        HookParticleSystemRender(il, true);
    }

    private static void ParticleSystemOnUpdate(On.Monocle.ParticleSystem.orig_Update orig, ParticleSystem self)
    {
        orig(self);
        if (new DynamicData(self).TryGet("LuckyHelper_Entities", out Entity[] lst))
        {
            for (var i = 0; i < lst.Length; i++)
            {
                if (!self.particles[i].Active)
                    lst[i] = null;
            }
        }
    }

    public static Color OrigParticleColor;

    private static void HookParticleSystemRender(ILContext il, bool withAlpha)
    {
        ILCursor cursor = new ILCursor(il);

        if (cursor.TryGotoNext(ins => ins.MatchLdloca(2)
            ))
        {
            // int i
            cursor.EmitLdarg0();
            cursor.EmitLdloc(1);

            // 尝试修改颜色
            cursor.EmitDelegate<Func<ParticleSystem, int, bool>>((particleSystem, index) =>
            {
                OrigParticleColor = particleSystem.particles[index].Color;
                if (new DynamicData(particleSystem).TryGet("LuckyHelper_Entities", out Entity[] lst))
                {
                    if (lst[index] != null && EntityToModifier.TryGetValue(lst[index], out ColorModifierComponent modifier) && modifier.AffectParticle)
                    {
                        float alpha = (particleSystem.particles[index].Color.A / 255f);
                        particleSystem.particles[index].Color = ApplyOverrideParticleColor(particleSystem.particles[index].Color, alpha);
                        return true;
                    }
                }

                return false;
            });

            var renderLabel = cursor.DefineLabel();
            // 如果没有修改颜色就跳过重新赋值的部分(性能优化?
            cursor.EmitBrfalse(renderLabel);
            cursor.EmitLdloc(0);
            cursor.EmitLdloc(1);
            cursor.EmitLdelemAny(typeof(Monocle.Particle));
            cursor.EmitStloc2();

            cursor.MarkLabel(renderLabel);

            if (withAlpha)
                cursor.GotoNext(ins => ins.MatchCall(typeof(Monocle.Particle).GetMethod("Render", new Type[1]
                {
                    typeof(float)
                })));
            else
                cursor.GotoNext(ins => ins.MatchCall(typeof(Monocle.Particle).GetMethod("Render", new Type[0])));
            cursor.Index += 1;
            // int i
            cursor.EmitLdarg0();
            cursor.EmitLdloc(1);

            cursor.EmitDelegate<Action<ParticleSystem, int>>((particleSystem, index) => { particleSystem.particles[index].Color = OrigParticleColor; });
        }
    }

    private static Color ApplyOverrideParticleColor(Color color, float alpha)
    {
        switch (CommonColorBlendMode)
        {
            case ColorBlendMode.Multiply:
                return color.Multiply(OverrideGeometryParticleColor) * alpha;
            case ColorBlendMode.Replace:
                return OverrideGeometryParticleColor * alpha;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static void BindEntityToParticle(Monocle.ParticleSystem self)
    {
        int preSlot = (self.nextSlot - 1 + self.particles.Length) % self.particles.Length;
        var dynamicData = new DynamicData(self);
        dynamicData.TryGet("LuckyHelper_Entities", out Entity[] lst);
        lst ??= new Entity[self.particles.Length];
        lst[preSlot] = CurrentUpdatingEntity;
        dynamicData.Set("LuckyHelper_Entities", lst);

        CurrentUpdatingEntity = null;
    }

    private static void ParticleSystemOnEmit_ParticleType_Vector2_float(On.Monocle.ParticleSystem.orig_Emit_ParticleType_Vector2_float orig, Monocle.ParticleSystem self,
        ParticleType type,
        Vector2 position, float direction)
    {
        orig(self, type, position, direction);
        BindEntityToParticle(self);
    }


    private static void ParticleSystemOnEmit_ParticleType_Vector2_Color_float(On.Monocle.ParticleSystem.orig_Emit_ParticleType_Vector2_Color_float orig,
        Monocle.ParticleSystem self,
        ParticleType type, Vector2 position, Color color, float direction)
    {
        orig(self, type, position, color, direction);
        BindEntityToParticle(self);
    }

    private static void ParticleSystemOnEmit_ParticleType_Vector2_Color(On.Monocle.ParticleSystem.orig_Emit_ParticleType_Vector2_Color orig, Monocle.ParticleSystem self,
        ParticleType type,
        Vector2 position, Color color)
    {
        orig(self, type, position, color);
        BindEntityToParticle(self);
    }

    private static void ParticleSystemOnEmit_ParticleType_Vector2(On.Monocle.ParticleSystem.orig_Emit_ParticleType_Vector2 orig, Monocle.ParticleSystem self, ParticleType type,
        Vector2 position)
    {
        orig(self, type, position);
        BindEntityToParticle(self);
    }

    private static void EntityOnAdded(On.Monocle.Entity.orig_Added orig, Entity self, Scene scene)
    {
        orig(self, scene);
        self.PreUpdate += entity => CurrentUpdatingEntity = entity;
    }

    public static Entity CurrentUpdatingEntity;

    #endregion

    #region Geometry

    private static Color ApplyOverrideGeometryColor(Color color)
    {
        if (!UseOverrideGeometryColor)
            return color;
        switch (CommonColorBlendMode)
        {
            case ColorBlendMode.Multiply:
                return color.Multiply(OverrideGeometryParticleColor);
            case ColorBlendMode.Replace:
                return OverrideGeometryParticleColor;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static void DrawOnPoint(On.Monocle.Draw.orig_Point orig, Vector2 at, Color color)
    {
        orig(at, ApplyOverrideGeometryColor(color));
    }


    private static void DrawOnLineAngle_Vector2_float_float_Color(On.Monocle.Draw.orig_LineAngle_Vector2_float_float_Color orig, Vector2 start, float angle, float length,
        Color color)
    {
        orig(start, angle, length, ApplyOverrideGeometryColor(color));
    }


    private static void DrawOnHollowRect_float_float_float_float_Color(On.Monocle.Draw.orig_HollowRect_float_float_float_float_Color orig, float x, float y, float width,
        float height,
        Color color)
    {
        orig(x, y, width, height, ApplyOverrideGeometryColor(color));
    }


    private static void DrawOnRect_float_float_float_float_Color(On.Monocle.Draw.orig_Rect_float_float_float_float_Color orig, float x, float y, float width, float height,
        Color color)
    {
        orig(x, y, width, height, ApplyOverrideGeometryColor(color));
    }

    private static void DrawOnLineAngle_Vector2_float_float_Color_float(On.Monocle.Draw.orig_LineAngle_Vector2_float_float_Color_float orig, Vector2 start, float angle,
        float length,
        Color color, float thickness)
    {
        orig(start, angle, length, ApplyOverrideGeometryColor(color), thickness);
    }

    private static void DrawOnLineAngle_float_float_float_float_Color(On.Monocle.Draw.orig_LineAngle_float_float_float_float_Color orig, float startX, float startY, float angle,
        float length,
        Color color)
    {
        orig(startX, startY, angle, length, ApplyOverrideGeometryColor(color));
    }

    private static void DrawOnCircle_float_float_float_Color_float_int(On.Monocle.Draw.orig_Circle_float_float_float_Color_float_int orig, float x, float y, float radius,
        Color color,
        float thickness, int resolution)
    {
        orig(x, y, radius, ApplyOverrideGeometryColor(color), thickness, resolution);
    }

    #endregion
}