using LuckyHelper.Handlers;
using LuckyHelper.Utils;

namespace LuckyHelper.Components;

// https://github.com/CommunalHelper/EeveeHelper
[Tracked(true)]
public class EntityContainer : Component
{
    public enum ContainMode
    {
        FlagChanged,
        RoomStart,
        Always,
        DelayedRoomStart
    }

    public List<IEntityHandler> Contained = new();
    public Dictionary<Entity, List<IEntityHandler>> HandlersFor = new();
    public List<Tuple<string, int>> Blacklist = new();
    public List<Tuple<string, int>> Whitelist = new();
    public ContainMode Mode = ContainMode.RoomStart;
    public string ContainFlag;
    public bool NotFlag;
    public bool ForceStandardBehavior;
    public bool IgnoreContainerBounds;
    public bool WhitelistAll;

    public Func<Entity, bool> IsValid;
    public Func<Entity, bool> DefaultIgnored;
    public Action<IEntityHandler> OnAttach;
    public Action<IEntityHandler> OnDetach;

    public bool Attached;
    public bool CollideWithContained;

    private List<IEntityHandler> containedSaved = new();
    private bool updatedOnce;

    public EntityContainer() : base(true, true)
    {
    }

    public EntityContainer(EntityData data) : this()
    {
        if (data.Attr("whitelist").ToLower() == "all")
        {
            WhitelistAll = true;
        }
        else
        {
            Whitelist = ParseList(data.Attr("whitelist"));
        }

        Blacklist = ParseList(data.Attr("blacklist"));
        Mode = data.Enum("containMode", ContainMode.FlagChanged);
        var flag = EeveeUtils.ParseFlagAttr(data.Attr("containFlag"));
        ContainFlag = flag.Item1;
        NotFlag = flag.Item2;
        ForceStandardBehavior = data.Bool("forceStandardBehavior", true);
        IgnoreContainerBounds = data.Bool("ignoreContainerBounds");
    }

    public override void EntityAwake()
    {
        base.EntityAwake();

        Attached = string.IsNullOrEmpty(ContainFlag) || SceneAs<Level>().Session.GetFlag(ContainFlag) != NotFlag;

        if (Attached && Mode != ContainMode.DelayedRoomStart)
        {
            AttachInside(true);
        }

        updatedOnce = false;
    }

    public override void Update()
    {
        base.Update();
        Cleanup();

        var newAttached = string.IsNullOrEmpty(ContainFlag) || SceneAs<Level>().Session.GetFlag(ContainFlag) != NotFlag;

        if (!updatedOnce)
        {
            if (newAttached && Mode == ContainMode.DelayedRoomStart)
            {
                Attached = newAttached;

                AttachInside(true);
            }

            updatedOnce = true;
        }

        if (Mode != ContainMode.Always)
        {
            if (newAttached != Attached)
            {
                Attached = newAttached;

                if (Attached)
                {
                    AttachInside();
                }
                else
                {
                    DetachAll();
                }
            }
        }
        else
        {
            var attachChanged = newAttached != Attached;

            Attached = newAttached;


            if (Attached)
            {
                DetachOutside();
                AttachInside();
            }
            else if (attachChanged)
            {
                DetachAll();
            }
        }
    }

    public virtual List<IEntityHandler> GetHandlersFor(Entity entity)
    {
        if (entity == null || !HandlersFor.ContainsKey(entity))
        {
            return new List<IEntityHandler>();
        }
        else
        {
            return HandlersFor[entity];
        }
    }

    public virtual bool HasHandlerFor<T>(Entity entity)
    {
        if (entity == null || !HandlersFor.ContainsKey(entity))
        {
            return false;
        }

        return HandlersFor[entity].Any(h => h is T);
    }

    public virtual bool IsFirstHandler(IEntityHandler handler)
    {
        if (!HandlersFor.ContainsKey(handler.Entity))
        {
            return true;
        }

        return HandlersFor[handler.Entity][0] == handler;
    }

    public virtual List<Entity> GetEntities()
    {
        var list = new List<Entity>();
        foreach (var handler in Contained)
        {
            if (!list.Contains(handler.Entity))
            {
                list.Add(handler.Entity);
            }
        }

        return list;
    }

    protected virtual void AddContained(IEntityHandler handler)
    {
        handler.OnAttach(this);
        // handler.Entity.AddOrAppendContainer(this.Entity as IContainer);
        Contained.Add(handler);

        if (!HandlersFor.TryGetValue(handler.Entity, out var handlers))
        {
            handlers = new List<IEntityHandler>();
            HandlersFor.Add(handler.Entity, handlers);
        }

        handlers.Add(handler);
    }

    protected virtual void RemoveContained(IEntityHandler handler)
    {
        Contained.Remove(handler);
        var handlers = HandlersFor[handler.Entity];
        handlers.Remove(handler);
        if (handlers.Count == 0)
        {
            HandlersFor.Remove(handler.Entity);
        }

        handler.OnDetach(this);
    }

    protected List<Tuple<string, int>> ParseList(string list)
    {
        return list.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(entry =>
        {
            var split = entry.Split(':');
            if (split.Length >= 2 && int.TryParse(split[1], out var count))
            {
                return Tuple.Create(split[0], count);
            }
            else
            {
                return Tuple.Create(entry, -1);
            }
        }).ToList();
    }

    protected virtual bool WhitelistCheck(Entity entity)
    {
        if (Blacklist.Any(pair => pair.Item1 == entity.GetType().Name && pair.Item2 == -1))
        {
            return false;
        }

        if (WhitelistAll)
        {
            return true;
        }

        if (Whitelist.Count == 0)
        {
            return !((DefaultIgnored?.Invoke(entity) ?? false) || entity is Player || entity is SolidTiles || entity is BackgroundTiles || entity is Decal || entity is Trigger ||
                     entity is WindController);
        }

        return Whitelist.Any(pair => pair.Item1 == entity.GetType().Name);
    }

    // 作用于同类实体中的特定实体的
    protected virtual bool WhitelistCheckCount(Entity entity, int count)
    {
        if (Blacklist.Any(pair => pair.Item1 == entity.GetType().Name && (pair.Item2 == -1 || count == pair.Item2)))
        {
            return false;
        }

        if (WhitelistAll || Whitelist.Count == 0)
        {
            return true;
        }

        return Whitelist.Any(pair => pair.Item1 == entity.GetType().Name && (pair.Item2 == -1 || count == pair.Item2));
    }

    protected virtual void AttachInside(bool first = false)
    {
        if (first || (Mode != ContainMode.RoomStart && Mode != ContainMode.DelayedRoomStart))
        {
            var counts = new Dictionary<Type, int>();
            foreach (var entity in Scene.Entities)
            {
                if (entity != Entity && WhitelistCheck(entity) && (IsValid?.Invoke(entity) ?? true))
                {
                    if (!counts.ContainsKey(entity.GetType()))
                    {
                        counts.Add(entity.GetType(), 0);
                    }

                    var anyInside = false;
                    var handlers = EntityHandler.CreateAll(entity, this, ForceStandardBehavior);
                    foreach (var handler in handlers)
                    {
                        if (IgnoreContainerBounds || handler.IsInside(this))
                        {
                            anyInside = true;

                            // if ((Mode != ContainMode.Always || !Contained.Contains(handler)) && WhitelistCheckCount(entity, counts[entity.GetType()] + 1))
                            if ((Mode != ContainMode.Always 
                                 || HandlersFor.GetValueOrDefault(handler.Entity, new List<IEntityHandler>()).All(h => h.GetType() != handler.GetType())) 
                                && WhitelistCheckCount(entity, counts[entity.GetType()] + 1))
                            {
                                AddContained(handler);
                                OnAttach?.Invoke(handler);
                            }
                        }
                    }

                    if (anyInside)
                    {
                        counts[entity.GetType()]++;
                    }
                }
            }
        }
        else
        {
            Cleanup();
            foreach (var handler in containedSaved)
            {
                AddContained(handler);
                OnAttach?.Invoke(handler);
            }

            containedSaved.Clear();
        }
    }

    protected virtual void DetachAll()
    {
        var lastContained = new List<IEntityHandler>(Contained);
        if (Mode == ContainMode.RoomStart || Mode == ContainMode.DelayedRoomStart)
        {
            containedSaved = lastContained;
        }

        foreach (var handler in lastContained)
        {
            RemoveContained(handler);
            OnDetach?.Invoke(handler);
        }
    }

    protected virtual void DetachOutside()
    {
        var toRemove = new List<IEntityHandler>();
        foreach (var handler in Contained)
        {
            if (!IgnoreContainerBounds && !handler.IsInside(this))
            {
                toRemove.Add(handler);
            }
        }

        foreach (var handler in toRemove)
        {
            RemoveContained(handler);
            OnDetach?.Invoke(handler);
        }
    }

    protected void Cleanup()
    {
        Contained.RemoveAll(e => e.Entity?.Scene == null);
    }

    public bool CheckCollision(Entity entity)
    {
        if (IgnoreContainerBounds)
        {
            return true;
        }

        if (entity.Collider != null)
        {
            var collidable = entity.Collidable;
            var parentCollidable = Entity.Collidable;
            entity.Collidable = true;
            Entity.Collidable = true;
            CollideWithContained = true;
            var result = Entity.CollideCheck(entity);
            CollideWithContained = false;
            entity.Collidable = collidable;
            Entity.Collidable = parentCollidable;
            return result;
        }
        else
        {
            return entity.X >= Entity.Left && entity.Y >= Entity.Top && entity.X <= Entity.Right && entity.Y <= Entity.Bottom;
        }
    }

    internal bool CheckDecal(Decal decal)
    {
        if (decal.textures.Count == 0)
        {
            return false;
        }

        var m = decal.textures[0];
        var r = new Rectangle((int)(decal.Position.X - (m.ClipRect.Width / 2)), (int)(decal.Position.Y - (m.ClipRect.Height / 2)), m.ClipRect.Width, m.ClipRect.Height);
        return Collide.CheckRect(Entity, r);
    }

    public Rectangle GetContainedBounds()
    {
        var bounds = new Rectangle();

        var first = true;
        foreach (var handler in Contained)
        {
            var rect = handler.GetBounds();
            bounds = first ? rect : Rectangle.Union(bounds, rect);
            first = false;
        }

        return bounds;
    }

    public void DestroyContained()
    {
        foreach (var handler in Contained)
        {
            handler.Destroy();
        }

        Contained.Clear();
    }
}