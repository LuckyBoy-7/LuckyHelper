using LuckyHelper.Components.EeveeLike;
using LuckyHelper.Extensions;
using LuckyHelper.Module;
using LuckyHelper.Modules;
using LuckyHelper.Utils;
using On.Celeste.Editor;
using LevelExit = On.Celeste.LevelExit;
using LevelTemplate = Celeste.Editor.LevelTemplate;

namespace LuckyHelper.Components;

// 游戏内 global, 真全局目前想不到怎么实现, 感觉只能做成 sl 那种感觉
[Tracked]
public class PersistentSingletonComponent(bool active = true, bool visible = true) : Component(active, visible)
{
    [Load]
    public static void Load()
    {
        On.Celeste.Level.LoadLevel += LevelOnLoadLevel;
        // https://github.com/krafs/Publicizer/issues/110, 找了一伯天的 bug, 结果还是没修好
        // https://github.com/JaThePlayer/FrostHelper/blob/master/Code/FrostHelper/FrostHelper.csproj#L31-L34, 最后选择了 BepInEx.AssemblyPublicizer.MSBuild
        // Everest.Events.AssetReload.OnBeforeReload += OnBeforeReload;
    }

    [Unload]
    public static void Unload()
    {
        On.Celeste.Level.LoadLevel -= LevelOnLoadLevel;
    }

    private static void LevelOnLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader)
    {
        var singletons = TypeToObjectsModule.BriefTypeToComponents[nameof(PersistentSingletonComponent)].Select(com => ((PersistentSingletonComponent)com).EntityID).ToHashSet();

        self.Session.DoNotLoad.AddRange(singletons);
        orig(self, playerIntro, isFromLoader);
        self.Session.DoNotLoad.RemoveWhere(elem => singletons.Contains(elem)); 
    }

    private bool EntityOrigPersistent;
    public EntityID EntityID;

    public override void Added(Entity entity)
    {
        base.Added(entity);
        EntityOrigPersistent = Entity.TagCheck(Tags.Persistent);
        if (!EntityOrigPersistent)
            Entity.AddTag(Tags.Persistent);

        EntityID = Entity.EntityID();
        LogUtils.LogDebug("===============");
        LogUtils.LogDebug(EntityID.ToString());
        LogUtils.LogDebug("===============");
    }

    public override void Removed(Entity entity)
    {
        // component 被 remove 的情况, 例如 container 将其 detach 的时候
        TryRestore(entity.Session());
        base.Removed(entity);
    }

    public override void Update()
    {
        base.Update();
        Entity.AddTag(Tags.Persistent);
    }

    private void TryRestore(Session session)
    {
        //todo: f6 的情况
        if (session == null)
            return;
        if (!EntityOrigPersistent)
        {
            Entity.RemoveTag(Tags.Persistent);
        }
    }

    public void DontLoadAnyMore()
    {
        Entity.Session().DoNotLoad.Add(EntityID);
    }
}