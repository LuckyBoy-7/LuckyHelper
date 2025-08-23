using System.Security.Cryptography.X509Certificates;
using LuckyHelper.Handlers;
using Celeste.Mod.Helpers;
using LuckyHelper.Entities.EeveeLike;
using LuckyHelper.Utils;
using MonoMod.Utils;

namespace LuckyHelper.Components.EeveeLike;

public class EntityContainerMover : EntityContainer
{
    public static bool LiftSpeedFix;
    public static bool DecalStaticMoverFix;

    private static Dictionary<Type, List<Type>> EntityHandlers = new();

    private static HashSet<string> IgnoredAnchors = new()
    {
        "Position", "ExactPosition", "TopLeft", "TopCenter", "TopRight", "Center", "CenterLeft", "CenterRight", "BottomLeft", "BottomCenter", "BottomRight"
    };

    private static HashSet<string> CommonAnchors = new()
    {
        "anchor", "anchorPosition", "start", "startPosition"
    };

    public Vector4 Padding;

    public bool FitContained;
    public bool IgnoreAnchors;
    public Action<Vector2, float, float> OnFit;
    public Action OnPreMove;
    public Action OnPostMove;


    public EntityContainerMover() : base()
    {
    }

    public EntityContainerMover(EntityData data, bool fitContained = true) : base(data)
    {
        if (fitContained)
        {
            FitContained = data.Bool("fitContained");
        }

        IgnoreAnchors = data.Bool("ignoreAnchors");
    }

    public override void Added(Entity entity)
    {
        base.Added(entity);
        
        entity.Add(new TransitionListener
        {
            OnOutBegin = () =>
            {
                if (!Entity.TagCheck(Tags.Persistent))
                {
                    Contained.RemoveAll(h => h.Entity != null && h.Entity.TagCheck(Tags.Persistent));
                }
            }
        });
    }

    public override void EntityAwake()
    {
        base.EntityAwake();

        if (!Attached)
        {
            Padding = new Vector4(Entity.Width / 2f, Entity.Height / 2f, Entity.Width / 2f, Entity.Height / 2f);
        }
    }

    public override void EntityRemoved(Scene scene)
    {
        base.EntityRemoved(scene);
        DestroyContained();
    }

    public override void Update()
    {
        if (Attached && FitContained)
        {
            var bounds = GetContainedBounds();
            var targetPos = new Vector2(bounds.X + Padding.X, bounds.Y + Padding.Y);
            var targetCorner = new Vector2(bounds.X + bounds.Width + Padding.Z, bounds.Y + bounds.Height + Padding.W);
            var targetWidth = targetCorner.X - targetPos.X;
            var targetHeight = targetCorner.Y - targetPos.Y;

            if (Entity.TopLeft != targetPos || Entity.BottomRight != targetCorner)
            {
                if (OnFit != null)
                {
                    OnFit(targetPos, targetWidth, targetHeight);
                }
                else
                {
                    Entity.Position = targetPos;
                    Entity.Collider.Width = targetWidth;
                    Entity.Collider.Height = targetHeight;
                }
            }
        }

        base.Update();
    }

    public override void AddContained(IEntityHandler handler)
    {
        base.AddContained(handler);

        if (IgnoreAnchors)
        {
            return;
        }

        if (handler is not IAnchorProvider)
        {
            var data = new DynamicData(handler.Entity);
            if (!data.TryGet<List<string>>("entityContainerAnchors", out _))
            {
                var anchors = FindAnchors(handler.Entity);
                if (anchors.Count > 0)
                {
                    data.Set("entityContainerAnchors", anchors);
                }
            }
        }
    }

    protected override void AttachInside(bool first = false)
    {
        base.AttachInside(first);

        var bounds = GetContainedBounds();
        Padding = new Vector4(Entity.Left - bounds.X, Entity.Top - bounds.Y, Entity.Right - (bounds.X + bounds.Width), Entity.Bottom - (bounds.Y + bounds.Height));
    }

    protected override void DetachOutside()
    {
        base.DetachOutside();

        var bounds = GetContainedBounds();
        Padding = new Vector4(Entity.Left - bounds.X, Entity.Top - bounds.Y, Entity.Right - (bounds.X + bounds.Width), Entity.Bottom - (bounds.Y + bounds.Height));
    }

    protected override void DetachAll()
    {
        base.DetachAll();

        Padding = new Vector4(Entity.Width / 2f, Entity.Height / 2f, Entity.Width / 2f, Entity.Height / 2f);
    }

    public void DoMoveAction(Action moveAction, Func<Entity, Vector2, Vector2?> liftSpeedGetter = null, bool liftSpeedFix = false)
    {
        Cleanup();
        var anchorOffsets = new Dictionary<Entity, Dictionary<string, Vector2>>();
        var collidable = new Dictionary<Entity, bool>();
        var startPosition = EeveeUtils.GetPosition(Entity);
        foreach (var handler in Contained)
        {
            if (handler is IMoveable moveable)
            {
                moveable.PreMove();
            }

            if (!collidable.ContainsKey(handler.Entity))
            {
                collidable.Add(handler.Entity, handler.Entity.Collidable);
                handler.Entity.Collidable = false;
            }
        }

        moveAction();
        var selfCollidable = Entity.Collidable;
        Entity.Collidable = false;
        OnPreMove?.Invoke();
        var newPosition = EeveeUtils.GetPosition(Entity);
        var moveOffset = newPosition - startPosition;
        var toMove = GetEntities();
        foreach (var handler in Contained)
        {
            if (collidable.ContainsKey(handler.Entity))
            {
                handler.Entity.Collidable = collidable[handler.Entity];
                collidable.Remove(handler.Entity);
            }

            if (!IgnoreAnchors)
            {
                // LogUtils.LogDebug(moveOffset.ToString());
                // LogUtils.LogDebug(handler.Entity.ToString());
                var anchors = GetAnchors(handler);
                var data = new InheritedDynData(handler.Entity);
                foreach (var anchor in anchors)
                {
                    data.Set(anchor, data.Get<Vector2>(anchor) + moveOffset);
                }
            }

            if (handler is IMoveable moveable)
            {
                // LogUtils.LogDebug("sladfkj");

                var liftSpeed = liftSpeedGetter?.Invoke(handler.Entity, moveOffset);
                if (moveable.Move(moveOffset, liftSpeed) && toMove.Contains(handler.Entity))
                {
                    toMove.Remove(handler.Entity);
                }
            }
        }

        if (liftSpeedFix)
        {
            LiftSpeedFix = true;
        }

        DecalStaticMoverFix = true;
        foreach (var entity in toMove)
        {
            if (entity is Platform platform)
            {
                var liftSpeed = liftSpeedGetter?.Invoke(entity, moveOffset);
                if (liftSpeed != null)
                {
                    platform.MoveH(moveOffset.X, liftSpeed.Value.X);
                    platform.MoveV(moveOffset.Y, liftSpeed.Value.Y);
                }
                else
                {
                    platform.MoveH(moveOffset.X);
                    platform.MoveV(moveOffset.Y);
                }
            }
            else
            {
                entity.Position += moveOffset;
            }
        }

        DecalStaticMoverFix = false;
        if (liftSpeedFix)
        {
            LiftSpeedFix = false;
        }

        Entity.Collidable = selfCollidable;
        OnPostMove?.Invoke();
    }

    public void DoIgnoreCollision(Action action)
    {
        var lastCollidable = new Dictionary<Entity, bool>();
        foreach (var entity in GetEntities())
        {
            lastCollidable.Add(entity, entity.Collidable);
            entity.Collidable = false;
        }

        action();
        foreach (var pair in lastCollidable)
        {
            pair.Key.Collidable = pair.Value;
        }
    }

    public List<string> GetAnchors(IEntityHandler handler)
    {
        return HandlerUtils.GetAs<IAnchorProvider, List<string>>(handler, (provider) => provider.GetAnchors(), (entity) =>
        {
            var data = new DynamicData(handler.Entity);
            var anchorNames = data.Get<List<string>>("entityContainerAnchors");
            if (anchorNames != null)
            {
                return anchorNames;
            }

            return null;
        }) ?? new List<string>();
    }

    public static List<string> FindAnchors(Entity entity)
    {
        var result = new List<string>();
        var data = new InheritedDynData(entity);
        foreach (var pair in data)
        {
            if (pair.Value is Vector2 vector && !IgnoredAnchors.Contains(pair.Key) && (vector == EeveeUtils.GetPosition(entity) || CommonAnchors.Contains(pair.Key)))
            {
                result.Add(pair.Key);
            }
        }

        return result;
    }

    public static void AddEntityHandler(Type entityType, Type handlerType)
    {
        foreach (var type in FakeAssembly.GetEntryAssembly().GetTypesSafe())
        {
            if (entityType.IsAssignableFrom(type))
            {
                // Console.WriteLine($"Registering handler {handlerType.Name} [{type.Name} : {entityType.Name}]");
                if (!EntityHandlers.ContainsKey(type))
                {
                    EntityHandlers.Add(type, new List<Type>());
                }

                EntityHandlers[type].Add(handlerType);
            }
        }
    }

    public static void AddEntityHandler<H>(Type entityType) where H : EntityHandler
    {
        AddEntityHandler(entityType, typeof(H));
    }

    public static void AddEntityHandler<E, H>() where E : Entity where H : EntityHandler
    {
        AddEntityHandler(typeof(E), typeof(H));
    }
}