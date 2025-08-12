using Celeste.Mod.Entities;
using System.Collections;
using System.Transactions;
using LuckyHelper.Extensions;
using LuckyHelper.Module;
using LuckyHelper.Utils;
using MonoMod.Cil;


namespace LuckyHelper.Components.EeveeLike;

[Tracked]
[CustomEntity("LuckyHelper/FollowerContainer")]
public class FollowerContainer : Actor, IContainer
{
    [Load]
    public static void Load()
    {
        On.Celeste.Leader.Update += LeaderOnUpdate;
        IL.Celeste.Leader.Update += LeaderOnUpdate;
    }


    [Unload]
    public static void Unload()
    {
        On.Celeste.Leader.Update -= LeaderOnUpdate;
        IL.Celeste.Leader.Update -= LeaderOnUpdate;
    }


    private static void LeaderOnUpdate(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);


        if (cursor.TryGotoNext(ins => ins.MatchStloc(4)
                // if (cursor.TryGotoNext(ins => ins.MatchStloc(1)
            ))
        {
            cursor.Index += 1;
            // 保存 index
            cursor.EmitLdloc(1);
            cursor.EmitLdloc(4);
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

        int i = 0;
        foreach (Follower follower in self.Followers)
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

    // private Dictionary<Entity, bool> wasPersistent = new();
    private Vector2 _respawnPosition;
    public Follower Follower;
    public EntityID ID;

    // collect
    private float _whiteAlpha;
    private bool _collecting;
    private string _collectFlag;

    // movement
    public int Interval = 0;
    public string LoseFlag;


    public FollowerContainer(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset + new Vector2(data.Width / 2f, data.Height / 2f))
    {
        Interval = data.Int("interval");
        LoseFlag = data.Attr("loseFlag");
        ID = id;
        Collider = new Hitbox(data.Width, data.Height);
        Collider.Position = new Vector2(-Width / 2f, -Height / 2f);
        AllowPushing = false;

        Depth = Depths.Top - 10;
        // Depth = 1000;
        _respawnPosition = Position;


        Add(_Container = new EntityContainerMover(data)
        {
            DefaultIgnored = e => e is FollowerContainer,
            OnFit = OnFit,
            OnAttach = handler =>
            {
                if (handler.Entity.Get<PersistentSingletonComponent>() == null)
                    handler.Entity.Add(new PersistentSingletonComponent());
            },
            OnDetach = handler => handler.Entity.Get<PersistentSingletonComponent>()?.RemoveSelf()
        });

        Add(Follower = new Follower(ID)); // 这里蔚蓝已经帮我们在 LoadLevel 的时候保证 FollowerContainer 不重复了
        Add(new PlayerCollider(OnPlayer));
        _collectFlag = data.Attr("collectFlag");
    }


    public override void Added(Scene scene)
    {
        base.Added(scene);
        // 如果放在 ctor 里此时还拿不到 EntityID, 因为得 new 完了 add 的时候才记录
        Add(new PersistentSingletonComponent());
    }

    private void OnPlayer(Player player)
    {
        // if (Follower.Leader == null && Container.Contained.Count > 0)
        if (Follower.Leader == null)
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
        if (Follower.Leader == null)
            return;
        
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