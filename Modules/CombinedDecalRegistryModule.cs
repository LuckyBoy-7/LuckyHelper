using System.Xml;
using Celeste.Mod.Registry.DecalRegistryHandlers;
using LuckyHelper.Module;

namespace LuckyHelper.Modules;

public class CombinedDecalRegistryModule
{
    public static readonly Dictionary<string, DecalRegistry.DecalInfo> RegisteredDecals = new Dictionary<string, DecalRegistry.DecalInfo>();

    [Load]
    public static void Load()
    {
        On.Celeste.Celeste.Initialize += CelesteOnInitialize;
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
    }

    internal static void LoadCombinedDecalRegistry()
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
        if (info.Handlers == null)
        {
            info.Handlers = new List<DecalRegistryHandler>(info.CustomProperties.Count);
            KeyValuePair<string, XmlAttributeCollection> keyValuePair = info.CustomProperties.Find((KeyValuePair<string, XmlAttributeCollection> p) => p.Key == "scale");
            if (keyValuePair.Value != null)
            {
                DecalRegistryHandler decalRegistryHandler = DecalRegistry.CreateHandlerOrNull(decalPath, keyValuePair.Key, keyValuePair.Value);
                if (decalRegistryHandler != null)
                {
                    info.Handlers.Add(decalRegistryHandler);
                }
            }

            foreach (KeyValuePair<string, XmlAttributeCollection> keyValuePair2 in info.CustomProperties)
            {
                string text;
                XmlAttributeCollection xmlAttributeCollection;
                keyValuePair2.Deconstruct(out text, out xmlAttributeCollection);
                string text2 = text;
                XmlAttributeCollection xmlAttributeCollection2 = xmlAttributeCollection;
                if (text2 != "scale")
                {
                    DecalRegistryHandler decalRegistryHandler2 = DecalRegistry.CreateHandlerOrNull(decalPath, text2, xmlAttributeCollection2);
                    if (decalRegistryHandler2 != null)
                    {
                        info.Handlers.Add(decalRegistryHandler2);
                    }
                }
            }
        }

        if (RegisteredDecals.ContainsKey(decalPath))
        {
            Logger.Verbose("Decal Registry", "Replaced decal " + decalPath);
        }
        else
        {
            Logger.Verbose("Decal Registry", "Registered decal " + decalPath);
        }

        RegisteredDecals[decalPath] = info;
    }
}