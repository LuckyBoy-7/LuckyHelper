using System.Collections;
using System.Runtime.CompilerServices;
using Celeste.Mod.Entities;
using LuckyHelper.Extensions;

namespace LuckyHelper.Entities.LootSpeedrun;

[CustomEntity("LuckyHelper/Loot")]
[Tracked]
public class Loot : Entity
{
    public int ID;
    public string CollectedID => ID + "_collected";
    private Sprite sprite;
    public Follower Follower;
    private Wiggler wiggler; // 捡到时的wiggle
    private BloomPoint bloom;
    private VertexLight light;
    private float wobble; // 上下浮动
    private Vector2 start; // 记录返回的初始位置
    private float collectTimer;
    private bool collected;
    public bool ReturnHomeWhenLost;


    // ------------------------------------
    private string spritePath;
    private int colliderWidth;
    private int colliderHeight;
    private int value;

    public Loot(EntityData data, Vector2 offset, EntityID gid)
    {
        spritePath = data.Attr("animationPath");
        colliderWidth = data.Int("colliderWidth");
        colliderHeight = data.Int("colliderHeight");
        value = data.Int("value");

        ReturnHomeWhenLost = true;
        ID = gid.ID;
        Position = (start = data.Position + offset);
        Depth = -100;
        Collider = new Hitbox(colliderWidth, colliderHeight, -colliderWidth / 2, -colliderHeight / 2);
        Add(new PlayerCollider(OnPlayer));
        Add(new MirrorReflection());
        Add(Follower = new Follower(gid, null, OnLoseLeader));
        Follower.FollowDelay = 0.3f;
    }


    public override void Added(Scene scene)
    {
        base.Added(scene);

        if (this.Session().GetFlag(CollectedID))
        {
            RemoveSelf();
            return;
        }

        sprite = new Sprite(GFX.Game, spritePath);
        sprite.AddLoop("idle", "", 0.08f);
        sprite.Play("idle");
        sprite.CenterOrigin();
        Add(sprite);

        Add(wiggler = Wiggler.Create(0.4f, 4f, delegate(float v) { sprite.Scale = Vector2.One * (1f + v * 0.35f); }));
        Add(bloom = new BloomPoint(1f, 12f));
        Add(light = new VertexLight(Color.White, 1f, 16, 24));

        if (((Level)scene).Session.BloomBaseAdd > 0.1f)
        {
            bloom.Alpha *= 0.5f;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Update()
    {
        if (!collected)
        {
            // 上下浮动
            this.wobble += Engine.DeltaTime * 4f;
            sprite.Y = (bloom.Y = (light.Y = (float)Math.Sin((double)this.wobble) * 2f));

            int followIndex = Follower.FollowIndex;
            // 从第一个开始结算
            if (Follower.Leader != null && Follower.DelayTimer <= 0f && IsFirstLoot)
            {
                Player player = Follower.Leader.Entity as Player;

                // 准备结算
                bool counting = false;
                if (player != null && player.Scene != null)
                {
                    var trigger = player.CollideFirst<LootCollectArea>();
                    if (trigger != null && (!trigger.CollectOnGround || player.OnGround()))
                        counting = true;
                }

                if (counting)
                {
                    collectTimer += Engine.DeltaTime;
                    if (collectTimer > 0.15f)
                    {
                        OnCollect();
                    }
                }
                else
                {
                    collectTimer = Math.Min(collectTimer, 0f);
                }
            }
            else
            {
                // 只要不是第一个, 就永远不收集
                if (followIndex > 0)
                {
                    collectTimer = -0.15f;
                }
            }
        }

        base.Update();
        if (Follower.Leader != null && Scene.OnInterval(0.08f))
        {
            ParticleType particleType = Strawberry.P_Glow;
            SceneAs<Level>().ParticlesFG.Emit(particleType, Position + Calc.Random.Range(-Vector2.One * 6f, Vector2.One * 6f));
        }
    }

    private bool IsFirstLoot
    {
        get
        {
            // 看看自己前面有没有loot
            for (int i = Follower.FollowIndex - 1; i >= 0; i--)
            {
                Loot loot = Follower.Leader.Followers[i].Entity as Loot;
                if (loot != null)
                {
                    return false;
                }
            }

            return true;
        }
    }

    public void OnPlayer(Player player)
    {
        if (Follower.Leader == null && !collected)
        {
            ReturnHomeWhenLost = true;

            Audio.Play("event:/game/general/strawberry_touch", Position);
            player.Leader.GainFollower(Follower);
            wiggler.Start();
            Depth = -1000000;
        }
    }

    public void OnCollect()
    {
        if (collected)
        {
            return;
        }

        collected = true;
        if (Follower.Leader != null)
        {
            Follower.Leader.LoseFollower(Follower);
        }

        Add(new Coroutine(CollectRoutine()));
    }

    private IEnumerator CollectRoutine()
    {
        Tag = Tags.TransitionUpdate;
        Depth = -2000010;

        Audio.Play("event:/game/general/strawberry_get", Position, "colour", 0, "count", 0);
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);

        // 被收集时的wiggle, 本来应该是动画的, 这样方便点
        float duration = 0.4f;
        float timer = duration;
        while (timer > 0)
        {
            timer = Math.Max(0, timer - Engine.DeltaTime);
            float k = Ease.BackOut(timer / duration);
            sprite.Scale = Vector2.One * k * 1.4f;
            bloom.Radius = 12 * k * 1.4f;

            yield return null;
        }

        this.Session().SetFlag(CollectedID);

        LootSpeedrunController controller = this.Tracker().GetEntity<LootSpeedrunController>();
        if (controller != null)
            controller.CurValue += value;

        RemoveSelf();
    }

    // 跑回去就好了
    private void OnLoseLeader()
    {
        if (!collected && ReturnHomeWhenLost)
        {
            Alarm.Set(
                this, 0.15f, delegate
                {
                    Vector2 vector = (start - Position).SafeNormalize();
                    float num = Vector2.Distance(Position, start);
                    float num2 = Calc.ClampedMap(num, 16f, 120f, 16f, 96f);
                    Vector2 vector2 = start + vector * 16f + vector.Perpendicular() * num2 * Calc.Random.Choose(1, -1);
                    SimpleCurve curve = new SimpleCurve(Position, start, vector2);
                    Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineOut, MathHelper.Max(num / 100f, 0.4f), true);
                    tween.OnUpdate = delegate(Tween f) { Position = curve.GetPoint(f.Eased); };
                    tween.OnComplete = delegate { Depth = 0; };
                    Add(tween);
                }
            );
        }
    }
}