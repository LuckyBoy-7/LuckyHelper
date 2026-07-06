using System.Runtime.CompilerServices;
using Celeste.Mod.EeveeHelper.Handlers;
using LuckyHelper.Components;
using LuckyHelper.Entities.Misc;
using LuckyHelper.Extensions;
using LuckyHelper.Module;
using LuckyHelper.Triggers;
using LuckyHelper.Utils;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil;
using MonoMod.Cil;
using On.FMOD;
using Backdrop = On.Celeste.Backdrop;
using BloomRenderer = On.Celeste.BloomRenderer;
using CassetteBlock = On.Celeste.CassetteBlock;
using CrystalStaticSpinner = On.Celeste.CrystalStaticSpinner;
using DisplacementRenderer = On.Celeste.DisplacementRenderer;
using Distort = On.Celeste.Distort;
using Godrays = On.Celeste.Godrays;
using LightingRenderer = On.Celeste.LightingRenderer;
using Player = On.Celeste.Player;

namespace LuckyHelper.Modules;

public class LayerViewerModule
{
    private static bool Enable;

    // 因为有些对象会在 Update 的时候改自己的 Visible, 所以我们得重新设置回去(因为咋们是开了预览才会运行, 所以不用担心性能开销)
    private static HashSet<Wrapper> UnvisibleWrappers = new();
    private static List<Wrapper> Sequence = new();
    private static int Index = 0;

    private static SimpleTextWithRectWrapped onOffText;

    private static SimpleTextWithRectWrapped lastBackdropContentText;

    private static HashSet<Type> UnvisibleTypes = new()
    {
        typeof(GameplayStats),
        typeof(WindController),
        typeof(Lightning),
        typeof(SeekerEffectsController),
        typeof(SimpleTextWithRectWrapped),
        typeof(WaterSurface),
    };

    private static HashSet<string> UnvisibleTypeNames = new()
    {
        "SelectedAreaEntity",
    };


    [Initialize]
    public static void Initialize()
    {
        onOffText = new SimpleTextWithRectWrapped("LayerViewerMode: On");
        onOffText.UseScreenPosition = true;
        onOffText.Position = new Vector2(1920, 0);
        onOffText.Justify = new Vector2(1, 0);
        onOffText.Scale = 0.8f;
        onOffText.Padding = 10;
        onOffText.Depth = -100000;
    }

    [Load]
    public static void Load()
    {
        On.Celeste.Player.Update += PlayerOnUpdate;
        On.Celeste.Backdrop.Update += BackdropOnUpdate;
        On.Celeste.Level.LoadLevel += LevelOnLoadLevel;
        On.Celeste.BloomRenderer.Apply += BloomRendererOnApply;
        On.Celeste.LightingRenderer.Render += LightingRendererOnRender;
        On.Celeste.Distort.Render += DistortOnRender;
        On.Celeste.Level.Update += LevelOnUpdate;
        On.Celeste.CassetteBlock.UpdateVisualState += CassetteBlockOnUpdateVisualState;
    }

    [Unload]
    public static void Unload()
    {
        On.Celeste.Player.Update -= PlayerOnUpdate;
        On.Celeste.Backdrop.Update -= BackdropOnUpdate;
        On.Celeste.Level.LoadLevel -= LevelOnLoadLevel;
        On.Celeste.BloomRenderer.Apply -= BloomRendererOnApply;
        On.Celeste.LightingRenderer.Render -= LightingRendererOnRender;
        On.Celeste.Distort.Render -= DistortOnRender;
        On.Celeste.Level.Update -= LevelOnUpdate;
        On.Celeste.CassetteBlock.UpdateVisualState -= CassetteBlockOnUpdateVisualState;
    }

    private static void CassetteBlockOnUpdateVisualState(CassetteBlock.orig_UpdateVisualState orig, Celeste.CassetteBlock self)
    {
        if (Enable && !self.Visible)
        {
            self.side.Visible = false;
            return;
        }

        orig(self);
    }

    private static void LevelOnUpdate(On.Celeste.Level.orig_Update orig, Level self)
    {
        orig(self);
        if (Enable)
            foreach (var wrapper in UnvisibleWrappers)
            {
                wrapper.Hide();
            }
    }

    private static void DistortOnRender(Distort.orig_Render orig, Texture2D source, Texture2D map, bool hasDistortion)
    {
        if (Enable && Engine.Scene is Level level && !level.Displacement.Visible)
        {
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null);
            Draw.SpriteBatch.Draw(source, Vector2.Zero, Color.White);
            Draw.SpriteBatch.End();
            return;
        }

        orig(source, map, hasDistortion);
    }


    private static void LightingRendererOnRender(LightingRenderer.orig_Render orig, Celeste.LightingRenderer self, Scene scene)
    {
        if (Enable && !self.Visible)
            return;
        orig(self, scene);
    }

    private static void BloomRendererOnApply(BloomRenderer.orig_Apply orig, Celeste.BloomRenderer self, VirtualRenderTarget target, Scene scene)
    {
        if (Enable && !self.Visible)
            return;
        orig(self, target, scene);
    }

    private static void LevelOnLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Celeste.Player.IntroTypes playerIntro, bool isFromLoader)
    {
        if (Enable)
        {
            Enable = false;
            OnEnd(self);
        }

        orig(self, playerIntro, isFromLoader);
    }

    private static void BackdropOnUpdate(Backdrop.orig_Update orig, Celeste.Backdrop self, Scene scene)
    {
        // 因为 backdrop 每帧都会更新 visible 状态, 所以当启用 LayerViewer 的时候需要关掉其 update
        if (Enable)
            return;
        orig(self, scene);
    }


    private static void PlayerOnUpdate(Player.orig_Update orig, Celeste.Player self)
    {
        if (LuckyHelperModule.Settings.LayerViewerSwitchButton.Pressed)
        {
            Enable = !Enable;
            if (Enable)
            {
                OnStart(self.level);
            }
            else
            {
                OnEnd(self.level);
            }
        }

        if (Enable)
        {
            if (LuckyHelperModule.Settings.LayerViewerStepForward.Pressed)
                StepForward(self.level);
            if (LuckyHelperModule.Settings.LayerViewerStepBackward.Pressed)
                StepBackward();

            if (LuckyHelperModule.Settings.LayerViewerQuickStepForward.Pressed)
                QuickStepForward(self.level);
            if (LuckyHelperModule.Settings.LayerViewerQuickStepBackward.Pressed)
                QuickStepBackward(self.level);
        }

        orig(self);
    }

    private static void QuickStepForward(Level level)
    {
        if (Index < Sequence.Count)
        {
            int currentGroupPriority = GetPriority(Sequence[Index].RawValue, level);
            while (Index < Sequence.Count && GetPriority(Sequence[Index].RawValue, level) == currentGroupPriority)
            {
                if (Index + 1 >= Sequence.Count || GetPriority(Sequence[Index].RawValue, level) != GetPriority(Sequence[Index + 1].RawValue, level))
                    CreateBottomCenterHint(GetCategory(Sequence[Index].RawValue, level, true), level);

                Sequence[Index].Show();
                Index++;
            }
        }
        else
        {
            StepToEndHint(level);
        }
    }

    private static void QuickStepBackward(Level level)
    {
        if (Index > 0 && Index <= Sequence.Count)
        {
            int currentGroupPriority = GetPriority(Sequence[Index - 1].RawValue, level);
            while (Index > 0 && GetPriority(Sequence[Index - 1].RawValue, level) == currentGroupPriority)
            {
                Index--;
                Sequence[Index].Hide();
            }
        }

        if (lastBackdropContentText != null)
        {
            lastBackdropContentText.RemoveSelf();
            lastBackdropContentText = null;
        }
    }

    private static void StepForward(Level level)
    {
        if (Index < Sequence.Count)
        {
            var wrapper = Sequence[Index];
            CreateBottomCenterHint(GetCategory(wrapper.RawValue, level, false), level);
            wrapper.Show();
            Index++;
        }
        else
        {
            StepToEndHint(level);
        }
    }

    private static void StepToEndHint(Level level)
    {
        CreateBottomCenterHint("END!", level);
    }

    private static void StepBackward()
    {
        if (Index > 0 && Index <= Sequence.Count)
        {
            Index--;
            var wrapper = Sequence[Index];
            wrapper.Hide();
        }

        if (lastBackdropContentText != null)
        {
            lastBackdropContentText.RemoveSelf();
            lastBackdropContentText = null;
        }
    }

    private static void OnEnd(Level level)
    {
        level.Remove(onOffText);
        foreach (Wrapper wrapper in Sequence)
        {
            wrapper.Show();
        }

        Sequence.Clear();
        UnvisibleWrappers.Clear();
    }


    private static void OnStart(Level level)
    {
        level.Add(onOffText);

        Index = 0;

        foreach (var backdrop in level.Background.Backdrops)
        {
            if (backdrop.Visible)
                Sequence.Add(new BackdropWrapper(backdrop, level));
        }

        Sequence.Add(new RendererWrapper(level.Lighting, level));
        Sequence.Add(new RendererWrapper(level.Bloom, level));
        if (level.Displacement.HasDisplacement(level))
            Sequence.Add(new RendererWrapper(level.Displacement, level));


        foreach (var entity in level.Entities)
        {
            if (entity.Visible && EntityIsVisibleValid(entity, level))
                Sequence.Add(new EntityWrapper(entity, level));
        }


        foreach (var backdrop in level.Foreground.Backdrops)
        {
            if (backdrop.Visible)
                Sequence.Add(new BackdropWrapper(backdrop, level));
        }

        Sequence.Sort((w1, w2) =>
        {
            int delta = GetPriority(w1.RawValue, level) - GetPriority(w2.RawValue, level);
            if (delta != 0)
                return delta;
            return string.Compare(w1.RawValue.GetType().Name, w2.RawValue.GetType().Name, StringComparison.Ordinal);
        });

        foreach (Wrapper wrapper in Sequence)
        {
            wrapper.Hide();
        }
    }

    private static bool EntityIsVisibleValid(Entity entity, Level level)
    {
        Type entityType = entity.GetType();
        if (UnvisibleTypes.Contains(entityType))
            return false;
        if (UnvisibleTypeNames.Contains(entityType.Name))
            return false;

        if (entity is ParticleSystem ps)
        {
            // 如果粒子系统有在跑的话那就将其算入观察对象之一
            foreach (var particle in ps.particles)
            {
                if (particle.Active)
                    return true;
            }

            return false;
        }

        if (entity is TrailManager tm)
        {
            // 如果粒子系统有在跑的话那就将其算入观察对象之一
            foreach (var snapshot in tm.snapshots)
            {
                if (snapshot != null)
                    return true;
            }

            return false;
        }

        if (entity is DustEdges)
        {
            return level.Tracker.GetComponents<DustEdge>().Count > 0;
        }

        if (entity is GlassBlockBg)
        {
            return level.Tracker.GetEntities<GlassBlock>().Count > 0;
        }

        if (entity is GrabbyIcon grabbyIcon)
        {
            return grabbyIcon.enabled;
        }

        if (entity is LightningRenderer lightningRenderer)
        {
            return lightningRenderer.list.Count > 0;
        }

        if (entity is MirrorSurfaces mirrorSurfaces)
        {
            return mirrorSurfaces.hasReflections;
        }

        if (entity is SeekerBarrierRenderer seekerBarrierRenderer)
        {
            return seekerBarrierRenderer.list.Count > 0;
        }

        if (entity is SpeedrunTimerDisplay speedrunTimerDisplay)
        {
            return speedrunTimerDisplay.DrawLerp > 0;
        }

        if (entity == level.HelperEntity)
            return false;

        // 吃心结算的半透明铺底用的 Panel, 这个时候 player 都不 update 了, 所以可以直接排除掉
        if (entity == level.FormationBackdrop)
            return false;


        return true;
    }

    private static string GetCategory(object obj, Level level, bool quickForward)
    {
        // 顺序: fgTile, player, bg, dark, entity, bgTile, bloom, bgDecal, fgDecal, level. displacement, fg
        if (obj == level.SolidTiles)
            return "Foreground Tiles";
        if (obj is Celeste.Player)
            return "Player";
        if (obj == level.BgTiles)
            return "Background Tiles";
        if (obj is Celeste.Backdrop backdrop)
        {
            if (backdrop.Renderer == level.Background)
            {
                if (quickForward)
                    return "Background Stylegrounds";
                return $"Background {GetBackdropContent(backdrop)}";
            }

            if (quickForward)
                return "Foreground Stylegrounds";
            return $"Foreground {GetBackdropContent(backdrop)}";
        }

        if (obj == level.Lighting)
            return $"Lighting | Darkness: {level.Lighting.Alpha}";

        if (obj == level.Bloom)
            return $"Bloom | Base: {level.Bloom.Base} Strength: {level.Bloom.Strength}";

        if (obj == level.Displacement)
            return "Displacement(handles localized distortion effects like bursts)";

        if (obj is Decal decal)
        {
            string message = decal.Depth > 0 ? "Background Decals" : "Foreground Decals";
            if (!quickForward)
            {
                string decalPath = decal.Name;
                message += $" | path: {decalPath}";
            }

            return message;
        }


        if (obj.GetType().Name == "DecalContainerRenderer")
            return "DecalContainerRenderer from FrostHelper";

        if (quickForward)
            return "Entities";
        return $"Entity: {GetDetailedEntityCategory(obj as Entity, level)}";
    }

    private static string GetDetailedEntityCategory(Entity entity, Level level)
    {
        Type entityType = entity.GetType();

        if (entity is ParticleSystem ps)
        {
            if (entity == level.Particles)
                return "ParticleSystem For Entities(Depth -8000)";
            if (entity == level.ParticlesFG)
                return "ParticleSystem For Foreground(Depth -50000)";
            if (entity == level.ParticlesBG)
                return "ParticleSystem For Background(Depth 8000)";
        }

        if (entity is Strawberry strawberry)
        {
            if (strawberry.Golden)
                return "Strawberry(Golden)";
            return "Strawberry";
        }

        return entityType.Name;
    }

    private static string GetBackdropContent(Celeste.Backdrop backdrop)
    {
        if (backdrop is Parallax parallax)
        {
            return $"Parallax | texture path: {parallax.Name}";
        }

        if (!string.IsNullOrEmpty(backdrop.Name))
            return $"{backdrop.GetType().Name}| Name:{backdrop.Name}";
        return backdrop.GetType().Name;
    }

    private static int GetPriority(object obj, Level level)
    {
        // 顺序: fgTile, player, bg, dark, entity, bgTile, bloom, bgDecal, fgDecal, DecalContainer. displacement, fg
        if (obj == level.SolidTiles)
            return 0;
        if (obj is Celeste.Player)
            return 1;
        if (obj == level.BgTiles)
            return 5;
        if (obj is Celeste.Backdrop backdrop)
        {
            if (backdrop.Renderer == level.Background)
                return 2;
            return 100;
        }

        if (obj == level.Lighting)
            return 3;

        if (obj == level.Bloom)
            return 6;

        if (obj == level.Displacement)
            return 10;

        if (obj is Decal decal)
        {
            if (decal.Depth > 0)
                return 7;
            return 8;
        }

        if (obj.GetType().Name == "DecalContainerRenderer")
            return 9;

        return 4;
    }

    private static void CreateBottomCenterHint(string content, Level level)
    {
        SimpleTextWithRectWrapped text = new SimpleTextWithRectWrapped(content)
        {
            Position = new Vector2(960, 1080),
            Justify = new Vector2(0.5f, 1),
            UseScreenPosition = true,
            Scale = 0.8f,
            Depth = -100000,
            Padding = 5
        };
        text.Add(new FadeOutComponent(5, f => text.Alpha = MathF.Min(1, f * 4), () => text.RemoveSelf()));
        level.Add(text);

        if (lastBackdropContentText != null)
            lastBackdropContentText.RemoveSelf();
        lastBackdropContentText = text;
    }

    #region Wrappers

    private abstract class Wrapper
    {
        public abstract object RawValue { get; }

        protected Level level;

        public Wrapper(Level level)
        {
            this.level = level;
        }

        public abstract void Show();
        public abstract void Hide();
    }

    private class EntityWrapper : Wrapper
    {
        public override object RawValue => entity;


        private Entity entity;

        public EntityWrapper(Entity entity, Level level) : base(level)
        {
            this.entity = entity;
        }


        public override void Show()
        {
            entity.Visible = true;
            UnvisibleWrappers.Remove(this);
        }

        public override void Hide()
        {
            entity.Visible = false;
            UnvisibleWrappers.Add(this);
        }
    }

    private class BackdropWrapper : Wrapper
    {
        public override object RawValue => backdrop;
        private Celeste.Backdrop backdrop;

        public BackdropWrapper(Celeste.Backdrop backdrop, Level level) : base(level)
        {
            this.backdrop = backdrop;
        }

        public override void Show()
        {
            backdrop.Visible = true;
            UnvisibleWrappers.Remove(this);
        }


        public override void Hide()
        {
            backdrop.Visible = false;
            UnvisibleWrappers.Add(this);
        }
    }

    private class RendererWrapper : Wrapper
    {
        public override object RawValue => renderer;
        private Renderer renderer;

        public RendererWrapper(Renderer renderer, Level level) : base(level)
        {
            this.renderer = renderer;
        }

        public override void Show()
        {
            renderer.Visible = true;
            UnvisibleWrappers.Remove(this);
        }

        public override void Hide()
        {
            renderer.Visible = false;
            UnvisibleWrappers.Add(this);
        }
    }

    #endregion
}