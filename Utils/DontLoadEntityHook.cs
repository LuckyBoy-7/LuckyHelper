using LuckyHelper.Module;
using Scene = On.Monocle.Scene;

namespace LuckyHelper.Utils;

public static class DontLoadEntityHook
{
    public static Dictionary<Entity, EntityID> CachedEntityToEntityID = new();
    public static bool LevelLoadingEntities;

    [Load]
    private static void Load()
    {
        On.Monocle.Scene.Add_Entity += SceneOnAdd_Entity;
        On.Celeste.Level.LoadLevel += LevelOnLoadLevel;
    }

    [Unload]
    private static void Unload()
    {
        On.Monocle.Scene.Add_Entity -= SceneOnAdd_Entity;
        On.Celeste.Level.LoadLevel -= LevelOnLoadLevel;
    }

    private static void LevelOnLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader)
    {
        // 清理应该删除的实体信息
        Dictionary<Entity, EntityID> tmp = new();
        foreach ((Entity entity, EntityID id) in CachedEntityToEntityID)
        {
            if (entity.Scene != null)
                tmp[entity] = id;
        }
        CachedEntityToEntityID = tmp;

        LevelLoadingEntities = true;
        orig(self, playerIntro, isFromLoader);
        LevelLoadingEntities = false;
    }

    private static void SceneOnAdd_Entity(Scene.orig_Add_Entity orig, Monocle.Scene self, Entity entity)
    {
        if (LevelLoadingEntities)
            CachedEntityToEntityID[entity] = Level._currentEntityId;
        orig(self, entity);
    }
}