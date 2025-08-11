using LuckyHelper.Extensions;
using LuckyHelper.Module;

namespace LuckyHelper.Components;

// 游戏内 global, 全局的话太麻烦了, 也没必要
[Tracked]
public class SingletonComponent(bool active = true, bool visible = true) : Component(active, visible)
{
    [Load]
    public static void Load()
    {
        On.Celeste.LevelExit.ctor += LevelExitOnctor;
        Everest.Events.AssetReload.OnBeforeReload += OnBeforeReload;
    }

    private static void OnBeforeReload(bool silent)
    {
        RestoreSingletonCompoennt();
    }

    private static void RestoreSingletonCompoennt()
    {
        if (Engine.Scene is Level level)
            level.Tracker.GetComponents<SingletonComponent>().ForEach(component => ((SingletonComponent)component).Restore());
    }


    private static void CelesteOnReloadAssets(On.Celeste.Celeste.orig_ReloadAssets orig, bool levels, bool graphics, bool hires, AreaKey? area)
    {
        if (Engine.Scene is Level level)
            level.Tracker.GetComponents<SingletonComponent>().ForEach(component => ((SingletonComponent)component).Restore());

        orig(levels, graphics, hires, area);
    }


    [Unload]
    public static void Unload()
    {
        On.Celeste.LevelExit.ctor -= LevelExitOnctor;
        Everest.Events.AssetReload.OnBeforeReload -= OnBeforeReload;
    }

    private static void LevelExitOnctor(On.Celeste.LevelExit.orig_ctor orig, Celeste.LevelExit self, Celeste.LevelExit.Mode mode, Session session, HiresSnow snow)
    {
        if (self.mode == Celeste.LevelExit.Mode.SaveAndQuit)
        {
            // LogUtils.LogDebug("sldafkj");
            if (Engine.Scene is Level level)
                level.Tracker.GetComponents<SingletonComponent>().ForEach(component => ((SingletonComponent)component).Restore());
        }

        orig(self, mode, session, snow);
    }

    public override void Added(Entity entity)
    {
        base.Added(entity);
        Entity.AddTag(Tags.Global);

        if (Entity.EntityID().ID != -1)
        {
            Entity.Session().DoNotLoad.Add(Entity.EntityID());
        }
    }

    public override void Update()
    {
        base.Update();
        Entity.AddTag(Tags.Global);
    }

    public override void EntityRemoved(Scene scene)
    {
        base.EntityRemoved(scene);
        Restore();
    }

    private void Restore()
    {
        Entity.RemoveTag(Tags.Global);
        Entity.Session().DoNotLoad.Remove(Entity.EntityID());
    }
}