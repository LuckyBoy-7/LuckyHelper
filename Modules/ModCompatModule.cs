using LuckyHelper.Extensions;
using LuckyHelper.Module;
using LuckyHelper.Triggers;

namespace LuckyHelper.Modules;

public class ModCompatModule
{
    public static bool ExtendedVariantLoaded = false;

    private static EverestModuleMetadata _extendedVariantMetadata = new()
    {
        Name = "ExtendedVariantMode",
        Version = new Version("0.39.2")
    };


    public static bool VivHelperLoaded = false;

    private static EverestModuleMetadata _vivHelperMetadata = new()
    {
        Name = "VivHelper",
        Version = new Version("1.14.10")
    };


    public static bool EeveeHelperLoaded = false;

    private static EverestModuleMetadata _eeveeHelperMetadata = new()
    {
        Name = "EeveeHelper",
        Version = new Version("1.12.3")
    };

    public static bool CavernHelperLoaded = false;

    private static EverestModuleMetadata _cavernHelperMetadata = new()
    {
        Name = "CavernHelper",
        Version = new Version("1.3.7")
    };


    public static bool VortexHelperLoaded = false;

    private static EverestModuleMetadata _vortexHelperMetadata = new()
    {
        Name = "VortexHelper",
        Version = new Version("1.2.19")
    };
    
    public static bool CollbaUtils2Loaded = false;

    private static EverestModuleMetadata _collbaUtils2Metadata = new()
    {
        Name = "CollabUtils2",
        Version = new Version("1.11.1")
    };



    public static void Load()
    {
        ExtendedVariantLoaded = Everest.Loader.DependencyLoaded(_extendedVariantMetadata);
        VivHelperLoaded = Everest.Loader.DependencyLoaded(_vivHelperMetadata);
        EeveeHelperLoaded = Everest.Loader.DependencyLoaded(_eeveeHelperMetadata);
        CavernHelperLoaded = Everest.Loader.DependencyLoaded(_cavernHelperMetadata);
        VortexHelperLoaded = Everest.Loader.DependencyLoaded(_vortexHelperMetadata);
        CollbaUtils2Loaded = Everest.Loader.DependencyLoaded(_collbaUtils2Metadata);
    }

    public static void Unload()
    {
    }
}