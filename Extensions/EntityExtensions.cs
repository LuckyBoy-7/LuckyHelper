namespace LuckyHelper.Extensions;

public static class EntityExtensions
{
    public static Session Session(this Entity entity)
    {
        return entity.SceneAs<Level>().Session;
    }

    public static Tracker Tracker(this Entity entity)
    {
        return entity.Scene.Tracker;
    }
    
    public static Level Level(this Entity entity)
    {
        return entity.SceneAs<Level>();
    }

    public static T GetEntity<T>(this Entity entity) where T : Entity
    {
        return entity.Tracker().GetEntity<T>();
    }
    
    public static List<Entity> GetEntities<T>(this Entity entity) where T : Entity
    {
        return entity.Tracker().GetEntities<T>();
    }
}

