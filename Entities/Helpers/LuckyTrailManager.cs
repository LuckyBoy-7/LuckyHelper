using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework.Graphics;

[Tracked]
public class LuckyTrailManager : Entity
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public LuckyTrailManager()
    {
        snapshots = new Snapshot[64];
        Tag = Tags.Global;
        Depth = 10;
        base.Add(new BeforeRenderHook(BeforeRenderPatch));
        base.Add(new MirrorReflection());
    }

    public override void Removed(Scene scene)
    {
        Dispose();
        base.Removed(scene);
    }

    public override void SceneEnd(Scene scene)
    {
        Dispose();
        base.SceneEnd(scene);
    }

    private void Dispose()
    {
        for (int i = 0; i < buffers.Length; i++)
        {
            VirtualRenderTarget virtualRenderTarget = buffers[i];
            if (virtualRenderTarget != null)
            {
                virtualRenderTarget.Dispose();
            }

            buffers[i] = null;
        }

        buffers = new VirtualRenderTarget[snapshots.Length];
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void BeforeRender()
    {
        if (!dirty)
        {
            return;
        }

        if (buffer == null)
        {
            buffer = VirtualContent.CreateRenderTarget("lucky-trail-manager", 512, 512);
        }

        Engine.Graphics.GraphicsDevice.SetRenderTarget(buffer);
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, LightingRenderer.OccludeBlendState);
        for (int i = 0; i < snapshots.Length; i++)
        {
            if (snapshots[i] != null && !snapshots[i].Drawn)
            {
                Draw.Rect(0f, 0f, buffer.Width, buffer.Height, Color.Transparent);
            }
        }

        Draw.SpriteBatch.End();
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, RasterizerState.CullNone);
        for (int j = 0; j < snapshots.Length; j++)
        {
            if (snapshots[j] != null && !snapshots[j].Drawn)
            {
                Snapshot snapshot = snapshots[j];
                Vector2 vector = new Vector2(buffer.Width * 0.5f, buffer.Height * 0.5f) - snapshot.Position;
                if (snapshot.Hair != null)
                {
                    for (int k = 0; k < snapshot.Hair.Nodes.Count; k++)
                    {
                        List<Vector2> list = snapshot.Hair.Nodes;
                        int num = k;
                        list[num] += vector;
                    }

                    snapshot.Hair.Render();
                    for (int l = 0; l < snapshot.Hair.Nodes.Count; l++)
                    {
                        List<Vector2> list = snapshot.Hair.Nodes;
                        int num = l;
                        list[num] -= vector;
                    }
                }

                Vector2 scale = snapshot.Sprite.Scale;
                snapshot.Sprite.Scale = snapshot.SpriteScale;
                snapshot.Sprite.Position += vector;
                snapshot.Sprite.Render();
                snapshot.Sprite.Scale = scale;
                snapshot.Sprite.Position -= vector;
                snapshot.Drawn = true;
            }
        }

        Draw.SpriteBatch.End();
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, MaxBlendState);
        Draw.Rect(0f, 0f, buffer.Width, buffer.Height, new Color(1f, 1f, 1f, 1f));
        Draw.SpriteBatch.End();
        dirty = false;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Snapshot Add(Entity entity, Color color, float duration = 1f, bool frozenUpdate = false, bool useRawDeltaTime = false)
    {
        Image image = entity.Get<PlayerSprite>();
        if (image == null)
        {
            image = entity.Get<Sprite>();
        }

        PlayerHair playerHair = entity.Get<PlayerHair>();
        return Add(entity.Position, image, playerHair, image.Scale, color, entity.Depth + 1, duration, frozenUpdate, useRawDeltaTime);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Snapshot Add(Entity entity, Vector2 scale, Color color, float duration = 1f)
    {
        Image image = entity.Get<PlayerSprite>();
        if (image == null)
        {
            image = entity.Get<Sprite>();
        }

        PlayerHair playerHair = entity.Get<PlayerHair>();
        return Add(entity.Position, image, playerHair, scale, color, entity.Depth + 1, duration);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Snapshot Add(Vector2 position, Image image, Color color, int depth, float duration = 1f)
    {
        return Add(position, image, null, image.Scale, color, depth, duration);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Snapshot Add(Vector2 position, Image sprite, PlayerHair hair, Vector2 scale, Color color, int depth, float duration = 1f,
        bool frozenUpdate = false, bool useRawDeltaTime = false)
    {
        LuckyTrailManager trailManager = Engine.Scene.Tracker.GetEntity<LuckyTrailManager>();
        if (trailManager == null)
        {
            trailManager = new LuckyTrailManager();
            Engine.Scene.Add(trailManager);
        }

        for (int i = 0; i < trailManager.snapshots.Length; i++)
        {
            if (trailManager.snapshots[i] == null)
            {
                Snapshot snapshot = Engine.Pooler.Create<Snapshot>();
                snapshot.Init(trailManager, i, position, sprite, hair, scale, color, duration, depth, frozenUpdate, useRawDeltaTime);
                trailManager.snapshots[i] = snapshot;
                trailManager.dirty = true;
                Engine.Scene.Add(snapshot);
                return snapshot;
            }
        }

        return null;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Clear()
    {
        LuckyTrailManager entity = Engine.Scene.Tracker.GetEntity<LuckyTrailManager>();
        if (entity != null)
        {
            for (int i = 0; i < entity.snapshots.Length; i++)
            {
                if (entity.snapshots[i] != null)
                {
                    entity.snapshots[i].RemoveSelf();
                }
            }
        }
    }

    // Note: this type is marked as 'beforefieldinit'.
    [MethodImpl(MethodImplOptions.NoInlining)]
    static LuckyTrailManager()
    {
        MaxBlendState = new BlendState
        {
            ColorSourceBlend = Blend.DestinationAlpha,
            AlphaSourceBlend = Blend.DestinationAlpha
        };
    }

    public static Snapshot Add(Entity entity, Color color, float duration = 1f)
    {
        return Add(entity, color, duration, false);
    }

    private void BeforeRenderPatch()
    {
        if (!dirty)
        {
            return;
        }

        Snapshot[] array = snapshots;
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] != null && !array[i].Drawn)
            {
                dirty = true;
                snapshots = new[] { array[i] };
                VirtualRenderTarget[] array2 = buffers;
                int num = i;
                if (array2[num] == null)
                {
                    array2[num] = VirtualContent.CreateRenderTarget("lucky-trail-manager-snapshot-" + i, 512, 512);
                }

                buffer = buffers[i];
                BeforeRender();
            }
        }

        snapshots = array;
        dirty = false;
        buffer = null;
    }

    private static BlendState MaxBlendState;

    private const int size = 64;

    private const int columns = 8;

    private const int rows = 8;

    private Snapshot[] snapshots;

    private VirtualRenderTarget buffer;

    private bool dirty;

    private VirtualRenderTarget[] buffers = new VirtualRenderTarget[64];

    [Pooled]
    [Tracked]
    public class Snapshot : Entity
    {
        public LuckyTrailManager Manager;

        public Image Sprite;

        public Vector2 SpriteScale;

        public PlayerHair Hair;

        public int Index;

        public Color Color;

        public float Percent;

        public float Duration;

        public bool Drawn;

        public bool UseRawDeltaTime;

        public Vector2 Speed;

        public Snapshot()
        {
            Add(new MirrorReflection());
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Init(LuckyTrailManager manager, int index, Vector2 position, Image sprite, PlayerHair hair, Vector2 scale, Color color, float duration,
            int depth, bool frozenUpdate, bool useRawDeltaTime)
        {
            Tag = Tags.Global;
            if (frozenUpdate)
            {
                Tag |= Tags.FrozenUpdate;
            }

            Manager = manager;
            Index = index;
            Position = position;
            Sprite = sprite;
            SpriteScale = scale;
            Hair = hair;
            Color = color;
            Percent = 0f;
            Duration = duration;
            Depth = depth;
            Drawn = false;
            UseRawDeltaTime = useRawDeltaTime;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Update()
        {
            Position += Speed * Engine.DeltaTime;
            if (Duration <= 0f)
            {
                if (Drawn)
                {
                    RemoveSelf();
                }
            }
            else
            {
                if (Percent >= 1f)
                {
                    RemoveSelf();
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Render()
        {
            VirtualRenderTarget virtualRenderTarget = Manager.buffers[Index];
            Rectangle rectangle = new Rectangle(0, 0, 512, 512);
            if (virtualRenderTarget != null)
            {
                Draw.SpriteBatch.Draw(
                    virtualRenderTarget, Position, rectangle, Color, 0f,
                    new Vector2(virtualRenderTarget.Width, virtualRenderTarget.Height) * 0.5f, Vector2.One, SpriteEffects.None, 0f
                );
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Removed(Scene scene)
        {
            if (Manager != null)
            {
                Manager.snapshots[Index] = null;
            }

            base.Removed(scene);
        }

    }
}