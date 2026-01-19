using System.Xml;
using Celeste.Mod.Registry.DecalRegistryHandlers;
using LuckyHelper.Module;
using LuckyHelper.Utils;

namespace LuckyHelper.Modules;

public class CombinedDecalRegistryModule
{
    public static readonly Dictionary<string, DecalRegistry.DecalInfo> RegisteredDecals = new Dictionary<string, DecalRegistry.DecalInfo>();
    public const string CombinedDecalRegistry = "CombinedDecalRegistry";

    [Load]
    public static void Load()
    {
        On.Celeste.Celeste.Initialize += CelesteOnInitialize;
        NonConfilctContentLoaderHelper.RegisterContent(CombinedDecalRegistry, LoadCombinedDecalRegistry);
    }

    private static void CelesteOnInitialize(On.Celeste.Celeste.orig_Initialize orig, Celeste.Celeste self)
    {
        orig(self);
        LoadCombinedDecalRegistry();
        // Logger.Warn("Test", RegisteredDecals.Count.ToString());
    }

    [Unload]
    public static void Unload()
    {
        On.Celeste.Celeste.Initialize -= CelesteOnInitialize;
        NonConfilctContentLoaderHelper.UnregisterContent(CombinedDecalRegistry);
    }


    private static void LoadCombinedDecalRegistry(ModAsset modAsset)
    {
        if (modAsset.Type == typeof(AssetTypeXml))
            LoadModCombinedDecalRegistry(modAsset);
    }

    private static void LoadCombinedDecalRegistry()
    {
        foreach (ModContent modContent in Everest.Content.Mods)
        {
            if (modContent.Map.TryGetValue(modContent.Name + "_CombinedDecalRegistry", out ModAsset modAsset) && modAsset.Type == typeof(AssetTypeXml))
            {
                LoadModCombinedDecalRegistry(modAsset);
            }
        }
    }

    internal static void LoadModCombinedDecalRegistry(ModAsset decalRegistry)
    {
        bool factoryHasInited = DecalRegistry.PropertyHandlerFactories.Count > 0;
        if (!factoryHasInited)
            return;
        
        Logger.Debug("Combined Decal Registry", "Loading registry for " + decalRegistry.Source.Name);
        foreach (KeyValuePair<string, DecalRegistry.DecalInfo> keyValuePair in DecalRegistry.ReadDecalRegistryXml(decalRegistry))
        {
            string decalPath = keyValuePair.Key;
            DecalRegistry.DecalInfo decalInfo = keyValuePair.Value;
            RegisterDecal(decalPath, decalInfo);
        }
    }

    private static void RegisterDecal(string decalPath, DecalRegistry.DecalInfo info)
    {
        info.Handlers = new(info.CustomProperties.Count);

        // Apply "scale" first since it affects other properties.
        if (info.CustomProperties.Find(p => p.Key == "scale") is { Value: { } } scaleProp)
        {
            if (CreateHandlerOrNull(decalPath, scaleProp.Key, scaleProp.Value) is { } handler)
                info.Handlers.Add(handler);
        }

        foreach ((string propertyName, var xml) in info.CustomProperties)
        {
            if (propertyName != "scale" && CreateHandlerOrNull(decalPath, propertyName, xml) is { } handler)
                info.Handlers.Add(handler);
        }

        if (RegisteredDecals.ContainsKey(decalPath))
        {
            Logger.Verbose("Decal Registry", "Replaced decal id " + decalPath);
        }
        else
        {
            Logger.Verbose("Decal Registry", "Registered decal id " + decalPath);
        }

        RegisteredDecals[decalPath] = info;
    }

    private static DecalRegistryHandler CreateHandlerOrNull(string decalName, string propertyName, XmlAttributeCollection xmlAttributes)
    {
        if (!DecalRegistry.PropertyHandlerFactories.TryGetValue(propertyName, out var factory))
        {
            Logger.Warn("Decal Registry", $"Unknown property {propertyName} in decal {decalName}");
            return null;
        }

        var handler = factory();
        handler.Parse(xmlAttributes);
        return handler;
    }
}