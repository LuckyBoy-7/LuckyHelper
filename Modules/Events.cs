using System.Diagnostics;
using System.Reflection;
using LuckyHelper.Module;
using LuckyHelper.Utils;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using LevelLoader = On.Celeste.LevelLoader;

namespace LuckyHelper.Modules;

public static class Events
{
    private static Hook OnCelesteMapDataLoadHook;

    public delegate void OnMapDataLoadDelegate(MapData mapData);

    public static event OnMapDataLoadDelegate OnMapDataLoad;

    public const string MapDataChangedToken = "LuckyHelper_MapDataChanged";

    private static MapData lastMapData;

    [Load]
    public static void Load()
    {
        On.Celeste.Level.LoadLevel += LevelOnLoadLevel;
        On.Celeste.LevelLoader.LoadingThread += LevelLoaderOnLoadingThread;
        Everest.Events.AssetReload.OnReloadLevel += OnReloadLevel;
    }


    [Unload]
    public static void Unload()
    {
        On.Celeste.Level.LoadLevel -= LevelOnLoadLevel;
        On.Celeste.LevelLoader.LoadingThread -= LevelLoaderOnLoadingThread;
        Everest.Events.AssetReload.OnReloadLevel -= OnReloadLevel;

        OnCelesteMapDataLoadHook.Dispose();
        OnCelesteMapDataLoadHook = null;
        if (lastMapData != null)
        {
            DynamicData dd = DynamicData.For(lastMapData);
            dd.Data.Remove(MapDataChangedToken);
            lastMapData = null;
        }
    }

    private static void OnReloadLevel(Level level)
    {
        MapDataLoad(level.Session.MapData);
    }

    private static void MapDataLoad(MapData mapData)
    {
        lastMapData = mapData;
        OnMapDataLoad?.Invoke(mapData);
        DynamicData dynamicData = DynamicData.For(mapData);
        dynamicData.Set(MapDataChangedToken, true);
    }

    private static void LevelOnLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader)
    {
        MapData mapData = self.Session.MapData;
        UpdateTokenState(mapData);
        orig(self, playerIntro, isFromLoader);
    }

    private static void UpdateTokenState(MapData mapData)
    {
        DynamicData dynamicData = DynamicData.For(mapData);
        dynamicData.TryGet<bool?>(MapDataChangedToken, out var registered);
        if (registered == null || !registered.Value)
        {
            dynamicData.Set(MapDataChangedToken, true);
            MapDataLoad(mapData);
        }
    }

    private static void LevelLoaderOnLoadingThread(LevelLoader.orig_LoadingThread orig, Celeste.LevelLoader self)
    {
        MapData mapData = self.session.MapData;
        MapDataLoad(mapData);
        orig(self);
    }
}