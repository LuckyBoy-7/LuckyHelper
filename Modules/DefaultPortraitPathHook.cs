using System.Reflection;
using LuckyHelper.Extensions;
using LuckyHelper.Module;
using LuckyHelper.Triggers;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace LuckyHelper.Modules;

public class DefaultPortraitPathHook
{
    private static ILHook textboxCoroutineHook;
    private static ILHook miniTextboxCtorHook;

    [Load]
    public static void Load()
    {
        IL.Celeste.Textbox.ctor_string_Language_Func1Array += TextboxOnctor_string_Language_Func1Array;

        var methodInfo = typeof(Textbox).GetMethod("RunRoutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget();
        textboxCoroutineHook = new ILHook(methodInfo, ILHookDashCoroutine);

        var miniTextboxCtor = typeof(MiniTextbox).GetMethod("orig_ctor", BindingFlags.Public | BindingFlags.Instance);
        miniTextboxCtorHook = new(miniTextboxCtor, ILHookMiniTextboxCtor);
    }

    private static void ILHookMiniTextboxCtor(ILContext il)
    {
        HookDefaultMiniPortraitPath(il);
    }

    private static void ILHookDashCoroutine(ILContext il)
    {
        HookDefaultPortraitPath(il);
    }

    private static void TextboxOnctor_string_Language_Func1Array(ILContext il)
    {
        HookDefaultPortraitPath(il);
    }

    private static void HookDefaultPortraitPath(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);

        string path = "textbox/default";
        if (cursor.TryGotoNext(ins => ins.MatchLdstr(path)
            ))
        {
            cursor.Remove();
            cursor.EmitDelegate(() =>
            {
                LuckyHelperSession session = LuckyHelperModule.Session;
                if (session.LuckyHelperAreaMetadata != null)
                    return session.LuckyHelperAreaMetadata.DefaultPortraitPath;
                return path;
            });
        }
    }

    private static void HookDefaultMiniPortraitPath(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);

        string path = "textbox/default_mini";

        if (cursor.TryGotoNext(ins => ins.MatchLdstr(path)
            ))
        {
            cursor.Remove();
            cursor.EmitDelegate(() =>
            {
                LuckyHelperSession session = LuckyHelperModule.Session;
                if (session.LuckyHelperAreaMetadata != null)
                    return session.LuckyHelperAreaMetadata.DefaultMiniPortraitPath;
                return path;
            });
        }
    }

    [Unload]
    public static void Unload()
    {
        IL.Celeste.Textbox.ctor_string_Language_Func1Array -= TextboxOnctor_string_Language_Func1Array;
        textboxCoroutineHook.Dispose();
        miniTextboxCtorHook.Dispose();
    }
}