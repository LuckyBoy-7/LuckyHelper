using System.Reflection;
using LuckyHelper.Module;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace LuckyHelper.Modules;

public class DefaultTextboxPathHook
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
        HookDefaultMiniTextboxPath(il);
    }

    private static void ILHookDashCoroutine(ILContext il)
    {
        HookDefaultTextboxPath(il);
    }

    private static void TextboxOnctor_string_Language_Func1Array(ILContext il)
    {
        HookDefaultTextboxPath(il);
    }

    private static void HookDefaultTextboxPath(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);

        if (cursor.TryGotoNext(ins => ins.MatchLdstr("textbox/default")
            ))
        {
            cursor.Index += 1;
            cursor.EmitDelegate<Func<string, string>>((origPath) =>
            {
                LuckyHelperSession session = LuckyHelperModule.Session;
                if (session.LuckyHelperAreaMetadata != null)
                    return session.LuckyHelperAreaMetadata.DefaultTextboxPath;
                return origPath;
            });
        }
    }

    private static void HookDefaultMiniTextboxPath(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);

        if (cursor.TryGotoNext(ins => ins.MatchLdstr("textbox/default_mini")
            ))
        {
            cursor.Index += 1;
            cursor.EmitDelegate<Func<string, string>>((origPath) =>
            {
                LuckyHelperSession session = LuckyHelperModule.Session;
                if (session.LuckyHelperAreaMetadata != null)
                    return session.LuckyHelperAreaMetadata.DefaultMiniTextboxPath;
                return origPath;
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