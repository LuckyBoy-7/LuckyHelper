using Celeste.Mod.Entities;
using System.Collections;
using LuckyHelper.Module;
using LuckyHelper.Utils;


namespace LuckyHelper.Components.EeveeLike;

[Tracked]
[CustomEntity("LuckyHelper/FollowerContainer")]
public class FollowerContainer : Actor, IContainer
{
    [Load]
    public static void Load()
    {
        On.Celeste.Leader.Update += LeaderOnUpdate;
    }


    [Unload]
    public static void Unload()
    {
        On.Celeste.Leader.Update -= LeaderOnUpdate;
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
    private float whiteAlpha;
    private Vector2 respawnPosition;
    public Follower Follower;
    public EntityID ID;

    public FollowerContainer(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset + new Vector2(data.Width / 2f, data.Height / 2f))
    {
        ID = id;
        Collider = new Hitbox(data.Width, data.Height);
        Collider.Position = new Vector2(-Width / 2f, -Height / 2f);
        AllowPushing = false;

        Depth = Depths.Top - 10;
        // Depth = 1000;
        respawnPosition = Position;


        Add(_Container = new EntityContainerMover(data)
        {
            DefaultIgnored = e => e is FollowerContainer,
            OnFit = OnFit,
            OnAttach = handler => handler.Entity.Add(new SingletonComponent()),
            OnDetach = handler => handler.Entity.Get<SingletonComponent>().RemoveSelf()
        });

        Add(Follower = new Follower(ID));
        Add(new PlayerCollider(OnPlayer));
        // Add(new SingletonComponent());
    }

    private void OnPlayer(Player player)
    {
        if (Follower.Leader == null)
        {
            Audio.Play("event:/game/general/strawberry_touch", Position);
            player.Leader.GainFollower(Follower);
            Depth = -1000000;
        }
    }

    public override void Update()
    {
        base.Update();


        // LogUtils.LogDebug($"{nameof(Container.Contained.Count)}: " + Container.Contained.Count);
        // LogUtils.LogDebug($"{nameof(Position)}: " + Position);
    }

// public override void DebugRender(Camera camera)
// {
// Draw.Rect(Position - new Vector2(Width, Height) / 2, Width, Height, Color.Green);
// base.DebugRender(camera);
// }
    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        // foreach (FollowerContainer container in scene.Tracker.GetEntities<FollowerContainer>())
        // {
        // 	if (noDuplicate && container != this && container.ID.Key == ID.Key && container.Hold != null && container.Hold.IsHeld)
        // 	{
        // 		RemoveSelf();
        // 		return;
        // 	}
        // }
    }

    private void OnFit(Vector2 pos, float width, float height)
    {
        Position = new Vector2(pos.X + width / 2f, pos.Y + height / 2f);
        Collider.Position = new Vector2(-width / 2f, -height / 2f);
        Collider.Width = width;
        Collider.Height = height;
    }


    private IEnumerator DestroyRoutine()
    {
        Collidable = false;

        Audio.Play(SFX.game_10_glider_emancipate, Position);
        var tween = Tween.Create(Tween.TweenMode.Oneshot, null, 0.2f, true);
        tween.OnUpdate = (t) => whiteAlpha = t.Eased;
        Add(tween);
        yield return 0.2f;

        _Container.DoMoveAction(() => Position = respawnPosition);

        var tween2 = Tween.Create(Tween.TweenMode.Oneshot, null, 0.1f, true);
        tween2.OnUpdate = (t) => whiteAlpha = (1f - t.Eased);
        Add(tween2);
    }
}