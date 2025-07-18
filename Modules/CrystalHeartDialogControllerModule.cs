using System.Reflection;
using LuckyHelper.Entities.Misc;
using LuckyHelper.Extensions;
using LuckyHelper.Module;
using LuckyHelper.Triggers;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace LuckyHelper.Modules;

public class CrystalHeartDialogControllerModule
{
    private static ILHook heartGemCollectCoroutineHook;

    [Load]
    public static void Load()
    {
        var methodInfo = typeof(HeartGem).GetMethod("orig_CollectRoutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget();
        heartGemCollectCoroutineHook = new ILHook(methodInfo, ILHookHeartGemCollectCoroutine);
    }

    [Unload]
    public static void Unload()
    {
        heartGemCollectCoroutineHook.Dispose();
    }

    private static void ILHookHeartGemCollectCoroutine(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        if (cursor.TryGotoNext(
                ins => ins.MatchStloc(14),
                ins => ins.MatchLdloc(1),
                ins => ins.MatchLdloc(14),
                ins => ins.MatchLdloc(1)
            ))
        {
            cursor.Index += 2;
            cursor.EmitLdloc(14);
            cursor.EmitDelegate<Func<string, string>>(origDialog =>
            {
                Tracker tracker = Engine.Scene.Tracker;
                CrystalHeartDialogController controller = tracker.GetEntity<CrystalHeartDialogController>();
                List<HeartGem> crystalHearts = tracker.GetEntities<HeartGem>().Cast<HeartGem>().ToList();
                if (controller == null || controller.Dialogs.Count == 0)
                {
                    return origDialog;
                }

                HeartGem currentHeartGem = crystalHearts.Find(gem => gem.collected);
                int i = crystalHearts.IndexOf(currentHeartGem);
                i = Math.Min(controller.Dialogs.Count - 1, i);
                return Dialog.Clean(controller.Dialogs[i]);
            });
            cursor.EmitStloc(14);
        }
    }
}