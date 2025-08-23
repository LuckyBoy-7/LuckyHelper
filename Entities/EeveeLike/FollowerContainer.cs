using Celeste.Mod.Entities;
using System.Collections;
using System.Reflection;
using LuckyHelper.Components;
using LuckyHelper.Components.EeveeLike;
using LuckyHelper.Extensions;
using LuckyHelper.Handlers;
using LuckyHelper.Module;
using LuckyHelper.Modules;
using LuckyHelper.Utils;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using DashBlock = On.Celeste.DashBlock;


namespace LuckyHelper.Entities.EeveeLike;

[Tracked]
[CustomEntity("LuckyHelper/FollowerContainer")]
public class FollowerContainer : Actor, IContainer
{
    public static ILHook VivLeaderHook;

    [Load]
    public static void Load()
    {
        On.Celeste.Leader.Update += LeaderOnUpdate;

        if (ModCompatModule.VivHelperLoaded)
        {
            Type vivLeaderModule = Everest.Modules.First(mod => mod.GetType().ToString() == "VivHelper.VivHelperModule").GetType();
            VivLeaderHook = new ILHook(vivLeaderModule.GetMethod("newLeaderUpdate", BindingFlags.Static | BindingFlags.NonPublic), LeaderOnUpdate);
        }
        else
        {
            IL.Celeste.Leader.Update += LeaderOnUpdate;
        }

        On.Celeste.Player.Die += PlayerOnDie;
    }


    [Unload]
    public static void Unload()
    {
        On.Celeste.Leader.Update -= LeaderOnUpdate;
        IL.Celeste.Leader.Update -= LeaderOnUpdate;

        VivLeaderHook?.Dispose();
        VivLeaderHook = null;

        On.Celeste.Player.Die -= PlayerOnDie;
    }

    private static PlayerDeadBody PlayerOnDie(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats)
    {
        // 因为 retry 的时候不会调用 update, 所以这里手动解决所有的 persistent singleton component
        foreach (FollowerContainer entity in new List<Entity>(self.Tracker().GetEntities<FollowerContainer>()))
        {
            if (entity.DontDestroyAfterDetached && entity.HasDetached)
            {
                // entity.Get<Coroutine>().RunToEnd();
                entity.AddTag(Tags.Global);
                continue;
            }

            entity.Get<PersistentSingletonComponent>()?.RemoveSelf();
        }
        // foreach (PersistentSingletonComponent com in new List<Component>(self.Tracker().GetComponents<PersistentSingletonComponent>()))
        // {
        // com.RemoveSelf();
        // }

        return orig(self, direction, evenIfInvincible, registerDeathInStats);
    }


    private static void LeaderOnUpdate(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);

        int keyLocIndex = ModCompatModule.VivHelperLoaded ? 6 : 4;

        if (cursor.TryGotoNext(ins => ins.MatchStloc(keyLocIndex)
            ))
        {
            cursor.Index += 1;
            // 保存 index
            cursor.EmitLdloc(1);
            cursor.EmitLdloc(keyLocIndex);
            cursor.EmitDelegate<Func<int, Follower, int>>((index, follower) =>
            {
                if (follower.Entity is FollowerContainer container)
                {
                    return index + container.Interval; // 间隔
                }

                return index;
            });
            // 保存 index
            cursor.EmitStloc(1);
        }

        if (cursor.TryGotoNext(ins => ins.MatchLdcR8(0.009999999776482582)
            ))
        {
            cursor.Index += 1;
            cursor.EmitLdloc(keyLocIndex);
            cursor.EmitDelegate<Func<double, Follower, double>>((k, follower) =>
            {
                if (follower.Entity is FollowerContainer container)
                {
                    return container.FollowerInvSpeed;
                }

                return k;
            });
        }
    }

    private static void LeaderOnUpdate(On.Celeste.Leader.orig_Update orig, Leader self)
    {
        List<Vector2> origPositions = new();
        foreach (Follower follower in self.Followers)
        {
            if (follower.Entity is FollowerContainer container)
            {
                origPositions.Add(container.Position);
            }
        }

        orig(self);

        // 防止移动的时候把 player 挤死导致 lose follower 后 list 内容变化而报错
        var followers = new List<Follower>(self.Followers);
        int i = 0;
        foreach (Follower follower in followers)
        {
            if (follower.Entity is FollowerContainer container)
            {
                Vector2 targetPos = container.Position;
                container.Position = origPositions[i++];
                container._Container.DoMoveAction(() => container.Position = targetPos);
            }
        }
    }


    public EntityContainer Container => _Container;
    public EntityContainerMover _Container;

    public Follower Follower;
    public EntityID ID;

    // collect
    private float _whiteAlpha;
    private bool _collecting;
    private string _collectFlag;

    // movement
    public int Interval = 0;
    public string LoseFlag;
    public float FollowerInvSpeed;
    public string CanFollowFlag;
    public bool DontDestroyAfterDetached;
    public bool HasDetached;
    private Player _player;

    public FollowerContainerHelperEntity HelperEntity;


    public FollowerContainer(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset + new Vector2(data.Width / 2f, data.Height / 2f))
    {
        Interval = data.Int("interval");
        LoseFlag = data.Attr("loseFlag");
        CanFollowFlag = data.Attr("canFollowFlag");
        FollowerInvSpeed = 1 - Calc.Clamp(data.Float("followerSpeed"), 0, 1);
        DontDestroyAfterDetached = data.Bool("dontDestroyAfterDetached");
        ID = id;
        Collider = new Hitbox(data.Width, data.Height);
        Collider.Position = new Vector2(-Width / 2f, -Height / 2f);
        AllowPushing = false;

        Depth = Depths.Top - 10;
        // Depth = 1000;


        Add(_Container = new EntityContainerMover(data)
        {
            DefaultIgnored = e => e is FollowerContainer,
            OnFit = OnFit,
            OnAttach = handler =>
            {
                handler.Entity.AddNoDuplicatedComponent(new PersistentSingletonComponent(true));
                handler.OnAddPersistentSingletonComponent();
            },
            OnDetach = handler =>
            {
                handler.Entity.Get<PersistentSingletonComponent>()?.RemoveSelf();
                handler.OnRemovePersistentSingletonComponent();
            }
        });


        Add(Follower = new Follower(ID, OnGainLeader)); // 这里蔚蓝已经帮我们在 LoadLevel 的时候保证 FollowerContainer 不重复了
        Add(new PlayerCollider(OnPlayer));
        _collectFlag = data.Attr("collectFlag");
    }

    private void OnGainLeader()
    {
        HasDetached = false;
    }


    public override void Added(Scene scene)
    {
        base.Added(scene);

        // 方便在 EntityContainer 位置突变的时候还能正常的带着 contained entity 走, 因为对于传送之类的东西, 一个个适配太麻烦了(
        HelperEntity = new FollowerContainerHelperEntity(Position);
        scene.Add(HelperEntity);
        Container.AddContained(new EntityHandler(HelperEntity));
        HelperEntity.AddNoDuplicatedComponent(new PersistentSingletonComponent(true));

        // 如果放在 ctor 里此时还拿不到 EntityID, 因为得 new 完了 add 的时候才记录
        Add(new PersistentSingletonComponent());
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        _player = scene.Tracker.GetEntity<Player>();
    }

    private void OnPlayer(Player player)
    {
        // if (Follower.Leader == null && Container.Contained.Count > 0)
        if (Follower.Leader == null && (string.IsNullOrEmpty(CanFollowFlag) || this.Session().GetFlag(CanFollowFlag)) && !this.Session().GetFlag(LoseFlag))
        {
            Audio.Play("event:/game/general/strawberry_touch", Position);
            player.Leader.GainFollower(Follower);
            Depth = -1000000;
        }
    }

    private void OnFit(Vector2 pos, float width, float height)
    {
        Position = new Vector2(pos.X + width / 2f, pos.Y + height / 2f);
        Collider.Position = new Vector2(-width / 2f, -height / 2f);
        Collider.Width = width;
        Collider.Height = height;
    }

    public override void Update()
    {
        base.Update();

        // LogUtils.LogDebug($"{Container.HelperEntity.Position} { Position}");
        // 适配传送等情况
        if (Vector2.Distance(HelperEntity.Position, Position) > 0.1f)
        {
            Vector2 delta = Position - HelperEntity.Position;
            Position = HelperEntity.Position;
            _Container.DoMoveAction(() => Position += delta);
        }

        // 在跟随的时候 player 死亡正常卸载, 如果是 detach 或者普通情况, 那就一直 global, 反正 load 是让 session 管的
        // 对于 player retry 不会调用 update, 这里用 hook 解决
        if (_player != null && _player.Dead)
        {
            if (!(DontDestroyAfterDetached && HasDetached))
            {
                RemoveTag(Tags.Global);
            }
            else
            {
                AddTag(Tags.Global);
            }
        }
        else
        {
            AddTag(Tags.Global);
        }

        if (Follower.Leader == null)
        {
            return;
        }


        var session = this.Session();
        if (session.GetFlag(LoseFlag))
        {
            Follower.Leader.LoseFollower(Follower);
        }

        if (session.GetFlag(_collectFlag) && !_collecting)
        {
            Add(new Coroutine(CollectRoutine()));
        }
    }

    public override void Render()
    {
        base.Render();
        if (_collecting)
            Draw.Rect(Position - new Vector2(Width, Height) / 2, Width, Height, Color.White * _whiteAlpha);
    }


    public override void DebugRender(Camera camera)
    {
        base.DebugRender(camera);
        // 好像因为 tas 的缘故, 变成 follower 的物体不显示碰撞箱
        Draw.HollowRect(Position - new Vector2(Width, Height) / 2, Width, Height, Color.Yellow * 0.5f);
    }


    private IEnumerator CollectRoutine()
    {
        _collecting = true;
        Collidable = false;
        Follower.Leader.LoseFollower(Follower);

        Audio.Play(SFX.game_10_glider_emancipate, Position);

        foreach (var handler in Container.Contained)
        {
            handler.Entity.Get<PersistentSingletonComponent>().DontLoadAnyMore();
        }

        Get<PersistentSingletonComponent>().DontLoadAnyMore();

        // blink 动画
        int frames = 5;
        for (int i = 1; i <= frames; i++)
        {
            float percent = Calc.Map((float)i / frames, 0, 1, 0, 0.5f);
            _whiteAlpha = percent;
            yield return null;
        }

        foreach (var handler in Container.Contained)
        {
            handler.Entity.RemoveSelf();
        }


        // blink 动画
        frames = 10;
        for (int i = 1; i <= frames; i++)
        {
            float percent = Calc.Map(1 - (float)i / frames, 0, 1, 0, 0.5f);
            _whiteAlpha = percent;
            yield return null;
        }

        RemoveSelf();
    }
}