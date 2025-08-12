using Lucky.Kits.Collections;
using LuckyHelper.Extensions;
using LuckyHelper.Module;
using LuckyHelper.Utils;
using Engine = On.Monocle.Engine;
using LevelEnter = On.Celeste.LevelEnter;
using LevelLoader = On.Celeste.LevelLoader;


namespace LuckyHelper.Modules;

public static class TypeToObjectsModule
{
    public static DefaultDict<string, HashSet<Entity>> BriefTypeToEntities = new(() => new());
    public static DefaultDict<string, HashSet<Component>> BriefTypeToComponents = new(() => new());
    public static Dictionary<Entity, EntityID> CachedEntityToEntityID = new();
    public static bool LevelLoadingEntity;

    [Load]
    public static void Load()
    {
        On.Monocle.Entity.Added += EntityOnAdded;
        On.Monocle.Entity.Removed += EntityOnRemoved;
        On.Monocle.Component.Added += ComponentOnAdded;
        On.Monocle.Component.Removed += ComponentOnRemoved;
        On.Celeste.LevelLoader.ctor += LevelLoaderOnctor;
        On.Celeste.Level.LoadLevel += LevelOnLoadLevel;
        On.Monocle.EntityList.Add_Entity += EntityListOnAdd_Entity;
    }


    [Unload]
    public static void Unload()
    {
        On.Monocle.Entity.Added -= EntityOnAdded;
        On.Monocle.Entity.Removed -= EntityOnRemoved;
        On.Monocle.Component.Added -= ComponentOnAdded;
        On.Monocle.Component.Removed -= ComponentOnRemoved;
        On.Celeste.LevelLoader.ctor -= LevelLoaderOnctor;
        On.Celeste.Level.LoadLevel -= LevelOnLoadLevel;
        On.Monocle.EntityList.Add_Entity -= EntityListOnAdd_Entity;
    }

    private static void EntityListOnAdd_Entity(On.Monocle.EntityList.orig_Add_Entity orig, Monocle.EntityList self, Entity entity)
    {
        if (LevelLoadingEntity)
            CachedEntityToEntityID[entity] = Level._currentEntityId;

        orig(self, entity);
    }


    private static void LevelOnLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader)
    {
        // 清理应该删除的实体信息, 正常被卸载的实体 scene 是 null
        Dictionary<Entity, EntityID> tmp = new();

        foreach ((Entity entity, EntityID id) in CachedEntityToEntityID)
        {
            if (entity.Scene != null && entity.Scene is Level)
                tmp[entity] = id;
        }

        CachedEntityToEntityID = tmp;

        LevelLoadingEntity = true;
        orig(self, playerIntro, isFromLoader);
        LevelLoadingEntity = false;
    }

    private static void LevelLoaderOnctor(LevelLoader.orig_ctor orig, Celeste.LevelLoader self, Session session, Vector2? startPosition)
    {
        // LogUtils.LogDebug("go");
        BriefTypeToEntities.Clear();
        BriefTypeToComponents.Clear();
        CachedEntityToEntityID.Clear();
        orig(self, session, startPosition);
    }

    public static IEnumerable<Entity> IterEntitiesByTypes(HashSet<string> types)
    {
        foreach (string type in types)
        {
            foreach (var entity in BriefTypeToEntities[type])
            {
                yield return entity;
            }
        }
    }

    private static void ComponentOnAdded(On.Monocle.Component.orig_Added orig, Component self, Entity entity)
    {
        BriefTypeToComponents[self.BriefTypeName()].Add(self);
        orig(self, entity);
    }

    private static void ComponentOnRemoved(On.Monocle.Component.orig_Removed orig, Component self, Entity entity)
    {
        BriefTypeToComponents[self.BriefTypeName()].Remove(self);
        orig(self, entity);
    }


    private static void EntityOnAdded(On.Monocle.Entity.orig_Added orig, Entity self, Scene scene)
    {
        BriefTypeToEntities[self.BriefTypeName()].Add(self);
        orig(self, scene);
    }

    private static void EntityOnRemoved(On.Monocle.Entity.orig_Removed orig, Entity self, Scene scene)
    {
        // 好像正常卸载的时候(比如自杀的时候 UnloadEntities)不会调用 component 的 remove, 所以咋们在 entity removed 的时候调整一下字典
        foreach (var selfComponent in self.Components)
        {
            BriefTypeToComponents[selfComponent.BriefTypeName()].Remove(selfComponent);
        }

        BriefTypeToEntities[self.BriefTypeName()].Remove(self);
        orig(self, scene);
    }
}