using System.Diagnostics;
using System.Reflection;
using LuckyHelper.Module;
using LuckyHelper.Utils;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace LuckyHelper.Modules;

public static class Events
{
    private static Hook OnCelesteMapDataLoadHook;

    public delegate void OnMapDataLoadDelegate(MapData mapData);

    public static event OnMapDataLoadDelegate OnMapDataLoad;

    public const string AtlasPathReplacer_Registered_Token = "LuckyHelper_AtlasPathReplacer_Registered";

    private static MapData lastMapData;

    [Load]
    public static void Load()
    {
        On.Celeste.Level.LoadLevel += LevelOnLoadLevel;
        var mapDataLoadMethodInfo = typeof(MapData).GetMethod("Load", BindingFlags.Instance | BindingFlags.NonPublic);
        OnCelesteMapDataLoadHook = new Hook(mapDataLoadMethodInfo, OnCelesteMapDataLoad);
    }


    [Unload]
    public static void Unload()
    {
        On.Celeste.Level.LoadLevel -= LevelOnLoadLevel;

        OnCelesteMapDataLoadHook.Dispose();
        OnCelesteMapDataLoadHook = null;
        if (lastMapData != null)
        {
            DynamicData dd = DynamicData.For(lastMapData);
            dd.Data.Remove(AtlasPathReplacer_Registered_Token);
            lastMapData = null;
        }
    }

    private static void MapDataLoad(MapData mapData)
    {
        OnMapDataLoad?.Invoke(mapData);
    }

    private static void OnCelesteMapDataLoad(OnMapDataLoadDelegate orig, MapData mapData)
    {
        orig(mapData);
        // DynamicData.For(mapData).Set(AtlasPathReplacer_Registered_Token, true);
        // MapDataLoad(mapData);
    }

    private static void LevelOnLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader)
    {
        MapData mapData = self.Session.MapData;
        DynamicData dynamicData = DynamicData.For(mapData);
        dynamicData.TryGet<bool?>(AtlasPathReplacer_Registered_Token, out var registered);
        if (registered == null || !registered.Value)
        { 
            lastMapData = mapData;
            dynamicData.Set(AtlasPathReplacer_Registered_Token, true);
            MapDataLoad(mapData);
        }

        orig(self, playerIntro, isFromLoader);
    }
}