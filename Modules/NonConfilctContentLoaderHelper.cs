using LuckyHelper.Module;

namespace LuckyHelper.Modules;

public static class NonConfilctContentLoaderHelper
{
    private static Dictionary<string, Action<ModAsset>> registeredContentLoaders = new();

    public static void RegisterContent(string contentPath, Action<ModAsset> loadContentAction)
    {
        registeredContentLoaders[contentPath] = loadContentAction;
    }

    public static void UnregisterContent(string contentPath)
    {
        registeredContentLoaders.Remove(contentPath);
    }

    [Load]
    public static void Load()
    {
        // Everest.Content.OnUpdate += ContentOnOnUpdate;
        Everest.Events.AssetReload.OnAfterReload += AssetReloadOnOnAfterReload;
        On.Celeste.GameLoader.Begin += GameLoaderOnBegin;
    }


    [Unload]
    public static void UnLoad()
    {
        // Everest.Content.OnUpdate -= ContentOnOnUpdate;
        Everest.Events.AssetReload.OnAfterReload -= AssetReloadOnOnAfterReload;
        On.Celeste.GameLoader.Begin -= GameLoaderOnBegin;
    }

    private static void AssetReloadOnOnAfterReload(bool silent)
    {
        foreach (ModContent mod in Everest.Content.Mods)
        {
            foreach ((string contentPath, Action<ModAsset> loadAction) in registeredContentLoaders)
            {
                if (mod.Map.TryGetValue(contentPath, out ModAsset configAsset))
                {
                    loadAction(configAsset);
                }
            }
        }
    }

    /// <summary>
    /// 一开始加载的时候
    /// </summary>
    /// <param name="orig"></param>
    /// <param name="self"></param>
    private static void GameLoaderOnBegin(On.Celeste.GameLoader.orig_Begin orig, GameLoader self)
    {
        foreach (ModContent mod in Everest.Content.Mods)
        {
            foreach ((string contentPath, Action<ModAsset> loadAction) in registeredContentLoaders) 
            {
                if (mod.Map.TryGetValue(contentPath, out ModAsset configAsset))
                {
                    loadAction(configAsset);
                }
            }
        }

        orig(self);
    }

    /// <summary>
    /// 资源更新的时候
    /// </summary>
    /// <param name="oldAsset"></param>
    /// <param name="newAsset"></param>
    private static void ContentOnOnUpdate(ModAsset oldAsset, ModAsset newAsset)
    {
        if (newAsset == null) return;

        foreach ((string contentPath, Action<ModAsset> loadAction) in registeredContentLoaders)
        {
            if (newAsset.PathVirtual.StartsWith(contentPath))
            {
                loadAction(newAsset);
            }
        }
    }
}