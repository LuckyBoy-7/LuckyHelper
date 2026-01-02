using LuckyHelper.Entities.EeveeLike;
using LuckyHelper.Entities.Misc;
using LuckyHelper.Module;
using MonoMod.Cil;
using Dialog = On.Celeste.Dialog;

namespace LuckyHelper.Modules;

public static class MiscModule
{
    [Load]
    public static void Load()
    {
        On.Celeste.Dialog.Clean += DialogOnClean;
    }


    [Unload]
    public static void Unload()
    {
        On.Celeste.Dialog.Clean -= DialogOnClean;
    }

    public static string LastCleanedDialog = "";

    private static string DialogOnClean(Dialog.orig_Clean orig, string name, Language language)
    {
        LastCleanedDialog = name;
        return orig(name, language);
    }
}