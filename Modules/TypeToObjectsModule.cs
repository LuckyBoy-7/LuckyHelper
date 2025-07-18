using Lucky.Kits.Collections;
using LuckyHelper.Extensions;
using LuckyHelper.Module;


namespace LuckyHelper.Modules;

public static class TypeToObjectsModule
{
    public static DefaultDict<string, HashSet<Entity>> BriefTypeToEntities = new(() => new());
    public static DefaultDict<string, HashSet<Component>> BriefTypeToComponents = new(() => new());

    [Load]
    public static void Load()
    {
        On.Monocle.Entity.Added += EntityOnAdded;
        On.Monocle.Entity.Removed += EntityOnRemoved;
        On.Monocle.Component.Added += ComponentOnAdded;
        On.Monocle.Component.Removed += ComponentOnRemoved;
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
        BriefTypeToEntities[self.BriefTypeName()].Remove(self);
        orig(self, scene);
    }


    [Unload]
    public static void Unload()
    {
        On.Monocle.Entity.Added -= EntityOnAdded;
        On.Monocle.Entity.Removed -= EntityOnRemoved;
    }
}