#if DEBUG

using System.Collections;
using LuckyHelper.Components;
using LuckyHelper.Entities;
using LuckyHelper.Module;
using LuckyHelper.Utils;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace LuckyHelper.Modules;

public static class TestModule
{
    [Load]
    public static void Load()
    {
        // On.Celeste.Level.Render += LevelOnRender;
        // On.Celeste.BloomRenderer.Apply += BloomRendererOnApply;
    }

#pragma warning disable CL0003
    private static void BloomRendererOnApply(On.Celeste.BloomRenderer.orig_Apply orig, Celeste.BloomRenderer self, VirtualRenderTarget target, Scene scene)
    {
        // MTexture circle = GFX.Game["LuckyHelper/bloom_circle"];
        if (self.Strength > 0f)
        {
            Texture2D texture2D = GaussianBlur.Blur(target, GameplayBuffers.TempA, GameplayBuffers.TempB, 0f, true, GaussianBlur.Samples.Nine, 1f, GaussianBlur.Direction.Both, 1f);
            List<Component> components = scene.Tracker.GetComponents<BloomPoint>();
            List<Component> components2 = scene.Tracker.GetComponents<EffectCutout>();
            Engine.Instance.GraphicsDevice.SetRenderTarget(GameplayBuffers.TempA);
            Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
            if (self.Base < 1f)
            {
                Camera camera = (scene as Level).Camera;
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null,
                    camera.Matrix);
                float num = 1f / (float)self.gradient.Width;
                foreach (Component component in components)
                {
                    BloomPoint bloomPoint = component as BloomPoint;
                    if (bloomPoint.Visible && bloomPoint.Radius > 0f && bloomPoint.Alpha > 0f)
                    {
                        // self.gradient.DrawCentered(bloomPoint.Entity.Position + bloomPoint.Position, Color.White * bloomPoint.Alpha, bloomPoint.Radius * 2f * num);
                        self.gradient.DrawCentered(bloomPoint.Entity.Position + bloomPoint.Position, Color.Yellow, bloomPoint.Radius * 2f * num);
                        // circle.DrawCentered(bloomPoint.Entity.Position + bloomPoint.Position, Color.White * bloomPoint.Alpha, 0.4f);
                    }
                }
                Draw.SpriteBatch.End();
            }



            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Draw.Rect(-10f, -10f, 340f, 200f, Color.White * self.Base);
            Draw.SpriteBatch.End();
            
            Engine.Instance.GraphicsDevice.SetRenderTarget(LuckyHelperBuffers.TempC);
            Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
            Engine.Graphics.GraphicsDevice.Textures[0] = GameplayBuffers.TempB;
            Engine.Graphics.GraphicsDevice.Textures[1] = GameplayBuffers.TempA;
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone,
                LuckyHelperEffects.CustomBloomBlendEffect, Matrix.Identity);
            // Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BloomRenderer.BlurredScreenToMask);
            Draw.SpriteBatch.Draw(GameplayBuffers.TempB, Vector2.Zero, Color.White);
            Draw.SpriteBatch.End();
            (GameplayBuffers.TempA, LuckyHelperBuffers.TempC) = (LuckyHelperBuffers.TempC, GameplayBuffers.TempA);
            
            Engine.Instance.GraphicsDevice.SetRenderTarget(target);
            Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            // Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BloomRenderer.AdditiveMaskToScreen);
            int num2 = 0;
            while ((float)num2 < self.Strength)
            {
                float num3 = (((float)num2 < self.Strength - 1f) ? 1f : (self.Strength - (float)num2));
                Draw.SpriteBatch.Draw(GameplayBuffers.TempA, Vector2.Zero, Color.White * num3);
                num2++;
            }
            Draw.SpriteBatch.End();
        }
    }
#pragma warning restore CL0003
    public static BlendState CustomBlurredScreenToMask = new BlendState()
    {
        ColorSourceBlend = Blend.DestinationColor,
        ColorDestinationBlend = Blend.Zero,
        ColorBlendFunction = BlendFunction.Add,
        AlphaSourceBlend = Blend.Zero,
        AlphaDestinationBlend = Blend.One,
        AlphaBlendFunction = BlendFunction.Add
    };

    public static BlendState CustomAlphaReplace = new BlendState()
    {
        ColorSourceBlend = Blend.One,
        ColorDestinationBlend = Blend.Zero,
        ColorBlendFunction = BlendFunction.Add,
        AlphaSourceBlend = Blend.One,
        AlphaDestinationBlend = Blend.Zero,
        AlphaBlendFunction = BlendFunction.Add
        // ColorSourceBlend = Blend.Zero,
        // ColorDestinationBlend = Blend.SourceAlpha,
        // ColorBlendFunction = BlendFunction.Add,
        // AlphaSourceBlend = Blend.One,
        // AlphaDestinationBlend = Blend.Zero,
        // AlphaBlendFunction = BlendFunction.Add
    };
#pragma warning disable CL0003
    private static void LevelOnRender(On.Celeste.Level.orig_Render orig, Celeste.Level self)
    {
        Engine.Instance.GraphicsDevice.SetRenderTarget(GameplayBuffers.Gameplay);
        Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
        self.GameplayRenderer.Render(self);
        self.Lighting.Render(self);
        Engine.Instance.GraphicsDevice.SetRenderTarget(GameplayBuffers.Level);
        Engine.Instance.GraphicsDevice.Clear(self.BackgroundColor);
        self.Background.Render(self);
        Distort.Render(GameplayBuffers.Gameplay, GameplayBuffers.Displacement, self.Displacement.HasDisplacement(self));
        self.Bloom.Apply(GameplayBuffers.Level, self);
        self.Foreground.Render(self);
        Glitch.Apply(GameplayBuffers.Level, self.glitchTimer * 2f, self.glitchSeed, 6.2831855f);
        if (Engine.DashAssistFreeze)
        {
            PlayerDashAssist entity = self.Tracker.GetEntity<PlayerDashAssist>();
            if (entity != null)
            {
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null,
                    self.Camera.Matrix);
                entity.Render();
                Draw.SpriteBatch.End();
            }
        }

        if (self.flash > 0f)
        {
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null);
            Draw.Rect(-1f, -1f, 322f, 182f, self.flashColor * self.flash);
            Draw.SpriteBatch.End();
            if (self.flashDrawPlayer)
            {
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null,
                    self.Camera.Matrix);
                Player entity2 = self.Tracker.GetEntity<Player>();
                if (entity2 != null && entity2.Visible)
                {
                    entity2.Render();
                }

                Draw.SpriteBatch.End();
            }
        }

        Engine.Instance.GraphicsDevice.SetRenderTarget(null);
        Engine.Instance.GraphicsDevice.Clear(Color.Black);
        Engine.Instance.GraphicsDevice.Viewport = Engine.Viewport;
        Matrix matrix = Matrix.CreateScale(6f) * Engine.ScreenMatrix;
        Vector2 vector = new Vector2(320f, 180f);
        Vector2 vector2 = vector / self.ZoomTarget;
        Vector2 vector3 = ((self.ZoomTarget != 1f) ? ((self.ZoomFocusPoint - vector2 / 2f) / (vector - vector2) * vector) : Vector2.Zero);
        MTexture orDefault = GFX.ColorGrades.GetOrDefault(self.lastColorGrade, GFX.ColorGrades["none"]);
        MTexture orDefault2 = GFX.ColorGrades.GetOrDefault(self.Session.ColorGrade, GFX.ColorGrades["none"]);
        if (self.colorGradeEase > 0f && orDefault != orDefault2)
        {
            ColorGrade.Set(orDefault, orDefault2, self.colorGradeEase);
        }
        else
        {
            ColorGrade.Set(orDefault2);
        }

        float num = self.Zoom * ((320f - self.ScreenPadding * 2f) / 320f);
        Vector2 vector4 = new Vector2(self.ScreenPadding, self.ScreenPadding * 0.5625f);
        if (SaveData.Instance.Assists.MirrorMode)
        {
            vector4.X = -vector4.X;
            vector3.X = 160f - (vector3.X - 160f);
        }

        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, ColorGrade.Effect,
            matrix);
        Draw.SpriteBatch.Draw(GameplayBuffers.Level, vector3 + vector4, new Rectangle?(GameplayBuffers.Level.Bounds), Color.White, 0f, vector3, num,
            SaveData.Instance.Assists.MirrorMode ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
        Draw.SpriteBatch.End();
        if (self.Pathfinder != null && self.Pathfinder.DebugRenderEnabled)
        {
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null,
                self.Camera.Matrix * matrix);
            self.Pathfinder.Render();
            Draw.SpriteBatch.End();
        }

        self.SubHudRenderer.Render(self);
        if (((!self.Paused || !self.PauseMainMenuOpen) && self.wasPausedTimer >= 1f) || !Input.MenuJournal.Check || !self.AllowHudHide)
        {
            self.HudRenderer.Render(self);
        }

        if (self.Wipe != null)
        {
            self.Wipe.Render(self);
        }

        if (self.HiresSnow != null)
        {
            self.HiresSnow.Render(self);
        }
    }
#pragma warning restore CL0003


    [Unload]
    public static void Unload()
    {
        On.Celeste.Level.Render -= LevelOnRender;
        On.Celeste.BloomRenderer.Apply -= BloomRendererOnApply;
    }
}
#endif