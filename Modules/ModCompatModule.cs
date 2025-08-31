using LuckyHelper.Extensions;
using LuckyHelper.Module;
using LuckyHelper.Triggers;

namespace LuckyHelper.Modules;

public class ModCompatModule
{
    private static EverestModuleMetadata _extendedVariantMetadata = new()
    {
        Name = "ExtendedVariantMode",
        Version = new Version("0.39.2")
    };

    public static bool ExtendedVariantLoaded = false;

    private static EverestModuleMetadata _vivHelperMetadata = new()
    {
        Name = "VivHelper",
        Version = new Version("1.14.10")
    };

    public static bool ExCameraDynamicsLoaded = false;

    private static EverestModuleMetadata _exCameraDynamicsMetadata = new()
    {
        Name = "ExtendedCameraDynamics", 
        Version = new Version("1.1.1")
    };

    public static bool VivHelperLoaded = false;
    
    private static EverestModuleMetadata _frostHelperMetadata = new()
    {
        Name = "FrostHelper", 
        Version = new Version("1.70.4")
    };

    public static bool FrostHelperLoaded = false;
    
    private static EverestModuleMetadata _eeveeHelperMetadata = new()
    {
        Name = "EeveeHelper", 
        Version = new Version("1.12.3")
    };

    public static bool EeveeHelperLoaded = false;
    
    public static void Load()
    {
        ExtendedVariantLoaded = Everest.Loader.DependencyLoaded(_extendedVariantMetadata);
        VivHelperLoaded = Everest.Loader.DependencyLoaded(_vivHelperMetadata);
        ExCameraDynamicsLoaded = Everest.Loader.DependencyLoaded(_exCameraDynamicsMetadata);
        FrostHelperLoaded = Everest.Loader.DependencyLoaded(_frostHelperMetadata);
        EeveeHelperLoaded = Everest.Loader.DependencyLoaded(_eeveeHelperMetadata);
    }

    public static void Unload()
    {
    }
}