using System.Collections;
using Celeste.Mod.Entities;
using LuckyHelper.Entities.Misc;
using LuckyHelper.Extensions;
using LuckyHelper.Utils;
using Microsoft.Xna.Framework.Graphics;

namespace LuckyHelper.Triggers;

[CustomEntity("LuckyHelper/DisperseSpritesTrigger")]
[Tracked]
public class DisperseSpritesTrigger : Trigger
{
    private string disperseFlag;
    private string disperseAudioEvent;
    private float disperseDirX;
    private float disperseDirY;
    private bool disableEntityUpdate;
    private bool disableEntityVisible;
    private bool disableEntityCollision;
    private bool fadeOutLight;
    private bool fadeOutSound;
    private bool fadeOutTalk;
    private bool dontLoadAfterFade;
    private float delay;

    private VirtualRenderTarget buffer;
    private WhiteBlacklistChecker whiteBlacklistChecker;

    private bool hasDispersed;
    private bool dispersedOver;
    List<Entity> trackedEntities;
    private DustEdges dustEdges;
    private int id;

    public DisperseSpritesTrigger(EntityData data, Vector2 offset, EntityID entityId) : base(data, offset)
    {
        disperseFlag = data.Attr("disperseFlag");
        disperseAudioEvent = data.Attr("disperseAudioEvent");
        disperseDirX = data.Float("disperseDirX");
        disperseDirY = data.Float("disperseDirY");
        delay = data.Float("delay", -1);
        disableEntityUpdate = data.Bool("disableEntityUpdate");
        disableEntityVisible = data.Bool("disableEntityVisible");
        disableEntityCollision = data.Bool("disableEntityCollision", true);
        dontLoadAfterFade = data.Bool("dontLoadAfterFade");
        fadeOutLight = data.Bool("fadeOutLight");
        fadeOutSound = data.Bool("fadeOutSound");
        fadeOutTalk = data.Bool("fadeOutTalk");
        Depth = data.Int("depth");
        whiteBlacklistChecker = new WhiteBlacklistChecker(data);

        id = entityId.ID;
        Add(new BeforeRenderHook(BeforeRender));
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        dustEdges = new DustEdges(id);
        scene.Add(dustEdges);
    }

    public override void Removed(Scene scene)
    {
        buffer?.Dispose();
        buffer = null;
        base.Removed(scene);
    }

    private void BeforeRender()
    {
        if (dispersedOver)
            return;

        if (buffer == null)
        {
            buffer = VirtualContent.CreateRenderTarget("lucky-DisperseSpritesTrigger-" + id, GameplayBuffers.Gameplay.Width, GameplayBuffers.Gameplay.Height);
        }

        if (trackedEntities == null || trackedEntities.Count == 0)
            return;

        dustEdges.CustomBeforeRender();

        GameplayRenderer.Begin();
        Engine.Graphics.GraphicsDevice.SetRenderTarget(buffer);
        Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
        dustEdges.Render();
        foreach (Entity entity in trackedEntities)
        {
            if (entity.Visible && !entity.TagCheck(Tags.HUD | TagsExt.SubHUD))
            {
                entity.Render();
            }
        }

        GameplayRenderer.End();
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        if (string.IsNullOrEmpty(disperseFlag))
            DisperseSprite();
    }

    public override void Update()
    {
        base.Update();
        if (this.TriggeredByFlag(disperseFlag))
        {
            DisperseSprite();
        }
    }

    private void DisperseSprite()
    {
        if (hasDispersed)
            return;
        hasDispersed = true;

        trackedEntities = GetTrackedEntities();
        Add(new Coroutine(DisperseSpriteCoroutine(trackedEntities)));
    }

    private List<Entity> GetTrackedEntities()
    {
        List<Entity> trackedEntities = new();
        List<Entity> dustEntities = new();
        foreach (Entity entity in Scene.Entities)
        {
            bool inBound = entity.Collidable && CollideCheck(entity) || Collider.Collide(entity.Position);
            if (entity is not Trigger && whiteBlacklistChecker.Check(entity.GetType().Name) && inBound)
            {
                trackedEntities.Add(entity);
                if (entity.Get<DustEdge>() != null)
                    dustEntities.Add(entity);
            }
        }

        List<Entity> specialEntitiesBeforeRender = new();
        List<Entity> specialEntitiesAfterRender = new();

        if (dustEntities.Count != 0)
        {
            dustEdges.SetDust(dustEntities);
        }

        foreach (Entity entity in trackedEntities)
        {
            if (entity is SwapBlock swapBlock)
            {
                specialEntitiesBeforeRender.Add(swapBlock.path);
            }
            else if (entity is ZipMover zipMover)
            {
                specialEntitiesBeforeRender.Add(zipMover.pathRenderer);
            }
            else if (entity is MoveBlock moveBlock)
            {
                specialEntitiesBeforeRender.Add(moveBlock.border);
            }
        }

        foreach (Entity entity in trackedEntities)
        {
            if (entity.Get<DustGraphic>() is { } dustGraphic)
            {
                if (dustGraphic.eyes != null)
                    specialEntitiesAfterRender.Add(dustGraphic.eyes);
            }
        }

        specialEntitiesBeforeRender.AddRange(trackedEntities);
        specialEntitiesBeforeRender.AddRange(specialEntitiesAfterRender);

        return specialEntitiesBeforeRender;
    }


    private IEnumerator DisperseSpriteCoroutine(List<Entity> trackedEntities)
    {
        if (delay > 0)
            yield return delay;
        else
            yield return null; // 这一帧 update 刚触发, 但是得等 render 后填好数据我们才能用
        RenderTarget2D renderTarget = buffer.Target;
        int width = renderTarget.Width;
        int height = renderTarget.Height;

        Color[] pixels = new Color[width * height];
        renderTarget.GetData(pixels);
        Color GetPixelColor(int x, int y) => pixels[y * width + x];

        Level level = this.Level();
        level.Add(new DisperseImage(new Vector2(disperseDirX, disperseDirY), Depth, renderTarget, GetPixelColor));
        Audio.Play(disperseAudioEvent, Position);


        yield return null; // 我们得在一帧后改原对象的的状态, 不然 DisperseImage 还没画出来画面会闪一下

        foreach (var trackedEntity in trackedEntities)
        {
            if (disableEntityVisible)
                trackedEntity.Visible = false;
            if (disableEntityUpdate)
            {
                trackedEntity.Active = false;
                // Active 不会影响 TransitionUpdate, 所以为了让对象真的不更新, 这里把 tag 删了(比如圆刺就会出显示问题)
                trackedEntity.RemoveTag(Tags.TransitionUpdate);
            }

            if (disableEntityCollision)
                trackedEntity.Collidable = false;

            if (fadeOutLight)
            {
                var light = trackedEntity.Get<BloomPoint>();
                if (light != null)
                    Add(new Coroutine(FadeOutLight(light)));
            }

            if (fadeOutSound)
            {
                var soundSource = trackedEntity.Get<SoundSource>();
                if (soundSource != null)
                    Add(new Coroutine(FadeOutSound(soundSource)));
            }

            if (fadeOutTalk)
            {
                var talkcomponent = trackedEntity.Get<TalkComponent>();
                if (talkcomponent != null)
                {
                    talkcomponent.Enabled = false;
                }
            }

            if (dontLoadAfterFade)
            {
                if (trackedEntity.SourceId.Level != null)
                    this.Session().DoNotLoad.Add(trackedEntity.SourceId);
            }
        }
        
        if (dontLoadAfterFade)
        {
            var session = this.Session();
            session.DoNotLoad.Add(SourceId);
        }

        yield return 6;
        dispersedOver = true;
    }

    private IEnumerator FadeOutLight(BloomPoint light)
    {
        const float speed = 1;
        while (light.Alpha > 0)
        {
            light.Alpha = Calc.Approach(light.Alpha, 0, speed * Engine.DeltaTime);
            yield return null;
        }
    }

    private IEnumerator FadeOutSound(SoundSource sound)
    {
        const float speed = 0.5f;
        if (sound.instance == null)
            yield break;

        sound.instance.getVolume(out float volume, out float finalVolume);
        while (volume > 0)
        {
            sound.instance.setVolume(Calc.Approach(volume, 0, speed * Engine.DeltaTime));
            yield return null;
            sound.instance.getVolume(out volume, out finalVolume);
        }
    }

    public class DisperseImage : Entity
    {
        public DisperseImage(Vector2 direction, int depth, RenderTarget2D renderTarget, Func<int, int, Color> getPixelFunc)
        {
            Depth = depth;

            particles = new List<Particle>();
            float num = direction.Angle();
            for (int i = 0; i < renderTarget.Width; i++)
            {
                for (int j = 0; j < renderTarget.Height; j++)
                {
                    if (getPixelFunc(i, j).A == 0)
                        continue;
                    particles.Add(new Particle
                    {
                        Position = GameplayRenderer.instance.Camera.Position + new Vector2(i, j),
                        Direction = Calc.AngleToVector(num + Calc.Random.Range(-0.2f, 0.2f), 1f),
                        Sin = Calc.Random.NextFloat(6.2831855f),
                        Speed = Calc.Random.Range(0f, 4f),
                        Percent = 0f,
                        Duration = Calc.Random.Range(1f, 3f),
                        Color = getPixelFunc(i, j)
                    });
                }
            }
        }

        public override void Update()
        {
            Simulate();
        }

        private void Simulate()
        {
            bool flag = false;
            foreach (Particle particle in particles)
            {
                particle.Percent += Engine.DeltaTime / particle.Duration;
                particle.Position += particle.Direction * particle.Speed * Engine.DeltaTime;
                particle.Position += (float)Math.Sin(particle.Sin) * particle.Direction.Perpendicular() * particle.Percent * 4f * Engine.DeltaTime;
                particle.Speed += Engine.DeltaTime * (4f + particle.Percent * 80f);
                particle.Sin += Engine.DeltaTime * 4f;
                if (particle.Percent < 1f)
                {
                    flag = true;
                }
            }

            if (!flag)
            {
                RemoveSelf();
            }
        }

        public override void Render()
        {
            foreach (Particle particle in particles)
            {
                Draw.Point(particle.Position, particle.Color * (1f - particle.Percent));
            }
        }

        private List<Particle> particles;

        private class Particle
        {
            public Vector2 Position;

            public Vector2 Direction;

            public float Speed;

            public float Sin;

            public float Percent;

            public float Duration;

            public Color Color;
        }
    }

    [Tracked]
    public class DustEdges : Entity
    {
        VirtualRenderTarget buffer;
        private int id;

        public DustEdges(int id)
        {
            AddTag(Tags.Global | Tags.TransitionUpdate);
            Depth = -48;
            // Add(new BeforeRenderHook(BeforeRender));
            this.id = id;
        }

        private void CreateTextures()
        {
            DustNoiseFrom = VirtualContent.CreateTexture("lucky-dust-noise-a", 128, 72, Color.White);
            DustNoiseTo = VirtualContent.CreateTexture("lucky-dust-noise-b", 128, 72, Color.White);
            Color[] array = new Color[DustNoiseFrom.Width * DustNoiseTo.Height];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = new Color(Calc.Random.NextFloat(), 0f, 0f, 0f);
            }

            DustNoiseFrom.Texture_Safe.SetData(array);
            for (int j = 0; j < array.Length; j++)
            {
                array[j] = new Color(Calc.Random.NextFloat(), 0f, 0f, 0f);
            }

            DustNoiseTo.Texture_Safe.SetData(array);
        }

        public override void Update()
        {
            noiseEase = Calc.Approach(noiseEase, 1f, Engine.DeltaTime);
            if (noiseEase == 1f)
            {
                VirtualTexture dustNoiseFrom = DustNoiseFrom;
                DustNoiseFrom = DustNoiseTo;
                DustNoiseTo = dustNoiseFrom;
                noiseFromPos = noiseToPos;
                noiseToPos = new Vector2(Calc.Random.NextFloat(), Calc.Random.NextFloat());
                noiseEase = 0f;
            }
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Dispose();
        }

        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            Dispose();
        }

        public override void HandleGraphicsReset()
        {
            base.HandleGraphicsReset();
            Dispose();
        }

        private void Dispose()
        {
            if (DustNoiseFrom != null)
            {
                DustNoiseFrom.Dispose();
            }

            if (DustNoiseTo != null)
            {
                DustNoiseTo.Dispose();
            }
        }

        private List<DustEdge> dustEdges = new();

        public void SetDust(List<Entity> dustEntities)
        {
            dustEdges.Clear();
            foreach (var dustEntity in dustEntities)
            {
                if (dustEntity.Get<DustEdge>() is { } edge)
                {
                    dustEdges.Add(edge);
                }
            }
        }

        public void CustomBeforeRender()
        {
            // List<Component> components = Scene.Tracker.GetComponents<DustEdge>();
            hasDust = dustEdges.Count > 0;
            if (hasDust)
            {
                if (buffer == null)
                    buffer = VirtualContent.CreateRenderTarget("lucky-ResortDust-" + id, GameplayBuffers.Gameplay.Width, GameplayBuffers.Gameplay.Height);

                Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.TempA);
                Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null,
                    (Scene as Level).Camera.Matrix);
                foreach (DustEdge dustEdge in dustEdges)
                {
                    if (dustEdge.Visible && dustEdge.Entity.Visible)
                    {
                        dustEdge.RenderDust();
                    }
                }

                Draw.SpriteBatch.End();
                if (DustNoiseFrom == null || DustNoiseFrom.IsDisposed)
                {
                    CreateTextures();
                }

                Vector2 vector = FlooredCamera();
                Engine.Graphics.GraphicsDevice.SetRenderTarget(buffer);
                Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
                Engine.Graphics.GraphicsDevice.Textures[1] = DustNoiseFrom.Texture_Safe;
                Engine.Graphics.GraphicsDevice.Textures[2] = DustNoiseTo.Texture_Safe;
                GFX.FxDust.Parameters["colors"].SetValue(DustStyles.Get(Scene).EdgeColors);
                GFX.FxDust.Parameters["noiseEase"].SetValue(noiseEase);
                GFX.FxDust.Parameters["noiseFromPos"].SetValue(noiseFromPos + new Vector2(vector.X / 320f, vector.Y / 180f));
                GFX.FxDust.Parameters["noiseToPos"].SetValue(noiseToPos + new Vector2(vector.X / 320f, vector.Y / 180f));
                GFX.FxDust.Parameters["pixel"].SetValue(new Vector2(0.003125f, 0.0055555557f));
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, GFX.FxDust,
                    Matrix.Identity);
                Draw.SpriteBatch.Draw(GameplayBuffers.TempA, Vector2.Zero, Color.White);
                Draw.SpriteBatch.End();
            }
        }

        public override void Render()
        {
            if (hasDust)
            {
                Vector2 vector = FlooredCamera();
                Draw.SpriteBatch.Draw(buffer, vector, Color.White);
            }
        }

        private Vector2 FlooredCamera()
        {
            Vector2 position = (Scene as Level).Camera.Position;
            position.X = (int)Math.Floor(position.X);
            position.Y = (int)Math.Floor(position.Y);
            return position;
        }

        private bool hasDust;

        private float noiseEase;

        private Vector2 noiseFromPos;

        private Vector2 noiseToPos;

        private VirtualTexture DustNoiseFrom;

        private VirtualTexture DustNoiseTo;
    }
}