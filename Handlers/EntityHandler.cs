using LuckyHelper.Components;
using Celeste.Mod.Helpers;

namespace LuckyHelper.Handlers;

public class EntityHandler : IEntityHandler
{
    public struct Registry
    {
        public EntityHandlerIsValid IsValid;
        public EntityHandlerFactory Constructor;
    }

    public delegate bool EntityHandlerIsValid(Entity entity, EntityContainer container);

    public delegate IEntityHandler EntityHandlerFactory(Entity entity, EntityContainer container);

    public static Dictionary<Type, List<Registry>> TypeFactories = new();
    public static List<Registry> GeneralFactories = new();

    public static List<IEntityHandler> CreateAll(Entity entity, EntityContainer container, bool forceDefault = false)
    {
        var list = new List<IEntityHandler>();
        if (!forceDefault)
        {
            var objectType = entity.GetType();
            if (TypeFactories.ContainsKey(objectType))
            {
                list.AddRange(HandlerSelector(TypeFactories[objectType], entity, container));
            }

            list.AddRange(HandlerSelector(GeneralFactories, entity, container));
        }

        if (list.Count == 0 && DefaultInsideCheck(entity, container))
        {
            list.Add(new EntityHandler(entity));
        }

        return new List<IEntityHandler>(list);
    }

    public static void Register(Type objectType, EntityHandlerFactory factory, EntityHandlerIsValid isValid = null)
    {
        var registry = new Registry { Constructor = factory, IsValid = isValid ?? DefaultInsideCheck };
        if (!TypeFactories.TryGetValue(objectType, out var factoryList))
        {
            TypeFactories.Add(objectType, factoryList = new List<Registry>());
        }

        factoryList.Add(registry);
    }

    public static void Register<T>(EntityHandlerFactory factory, EntityHandlerIsValid isValid = null) where T : Entity
    {
        Register(typeof(T), factory, isValid);
    }

    public static void RegisterInherited(Type objectType, EntityHandlerFactory factory, EntityHandlerIsValid isValid = null)
    {
        foreach (var type in FakeAssembly.GetEntryAssembly().GetTypesSafe())
        {
            if (objectType.IsAssignableFrom(type))
            {
                Register(type, factory, isValid);
            }
        }
    }

    public static void RegisterInherited<T>(EntityHandlerFactory factory, EntityHandlerIsValid isValid = null) where T : Entity
    {
        RegisterInherited(typeof(T), factory, isValid);
    }

    public static void Register(EntityHandlerFactory factory, EntityHandlerIsValid isValid = null)
    {
        var registry = new Registry { Constructor = factory, IsValid = isValid ?? DefaultInsideCheck };
        GeneralFactories.Add(registry);
    }

    public static bool DefaultInsideCheck(Entity entity, EntityContainer container)
    {
        return container.CheckCollision(entity);
    }

    private static IEnumerable<IEntityHandler> HandlerSelector(List<Registry> registries, Entity entity, EntityContainer container)
    {
        foreach (var registry in registries)
        {
            var handler = registry.Constructor(entity, container);
            if (handler != null)
            {
                yield return handler;
            }
        }
    }

    // Impl

    public Entity Entity { get; }
    public EntityContainer Container { get; set; }

    public EntityHandler(Entity entity)
    {
        Entity = entity;
    }

    public virtual void OnAttach(EntityContainer container)
    {
        Container = container;
    }

    public virtual void OnDetach(EntityContainer container)
    {
        Container = null;
    }

    public virtual bool IsInside(EntityContainer container)
    {
        return container.CheckCollision(Entity);
    }

    public virtual Rectangle GetBounds()
    {
        return new Rectangle((int)Entity.Left, (int)Entity.Top, (int)Entity.Width, (int)Entity.Height);
    }

    public virtual void Destroy()
    {
        if (Entity is Platform platform)
        {
            platform.DestroyStaticMovers();
        }

        var follower = Entity.Get<Follower>();
        if (follower != null && follower.HasLeader)
        {
            follower.Leader.LoseFollower(follower);
        }

        Entity.RemoveSelf();
    }
}