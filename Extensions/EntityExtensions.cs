using LuckyHelper.Modules;

namespace LuckyHelper.Extensions;

public static class EntityExtensions
{
    public static Session Session(this Entity entity)
    {
        return entity.SceneAs<Level>()?.Session;
    }

    public static Tracker Tracker(this Entity entity)
    {
        return entity.Scene?.Tracker;
    }

    public static Level Level(this Entity entity)
    {
        return entity.SceneAs<Level>();
    }

    public static T GetEntity<T>(this Entity entity) where T : Entity
    {
        return entity.Tracker()?.GetEntity<T>();
    }

    public static List<Entity> GetEntities<T>(this Entity entity) where T : Entity
    {
        return entity.Tracker()?.GetEntities<T>();
    }

    public static string CurrentRoomName(this Entity entity)
    {
        return entity.Session().Level;
    }

    public static string GetCheckpointName(this Entity entity)
    {
        string res = AreaData.GetCheckpointName(entity.Session().Area, entity.Session().Level);
        if (res == null)
            // res = "StartCheckpoint";
            res = "";
        return res;
    }
    
    public static void AddNoDuplicatedComponent<T>(this Entity entity, T component) where T: Component
    {
        if (entity.Get<T>() == null)
            entity.Add(component);
    }
    
    public static Vector2 HalfSize(this Entity entity)
    {
        return new Vector2(entity.Width, entity.Height) / 2;
    }
}