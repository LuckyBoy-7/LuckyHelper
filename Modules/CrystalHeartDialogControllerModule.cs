using System.Reflection;
using LuckyHelper.Entities.Misc;
using LuckyHelper.Module;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using Level = On.Celeste.Level;

namespace LuckyHelper.Modules;

public class CrystalHeartDialogControllerModule
{
    private static ILHook heartGemCollectCoroutineHook;

    // 因为水晶之心被收集之后就不会再生成了, 所以只在进房间的时候加载一次也不会有问题
    private static Dictionary<HeartGem, int> currentRoomHeartGemToIndex = new();

    [Load]
    public static void Load()
    {
        var methodInfo = typeof(HeartGem).GetMethod("orig_CollectRoutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget();
        heartGemCollectCoroutineHook = new ILHook(methodInfo, ILHookHeartGemCollectCoroutine);

        On.Celeste.Level.LoadLevel += LevelOnLoadLevel;
    }


    [Unload]
    public static void Unload()
    {
        heartGemCollectCoroutineHook.Dispose();
        On.Celeste.Level.LoadLevel -= LevelOnLoadLevel;
    }

    private static void LevelOnLoadLevel(Level.orig_LoadLevel orig, Celeste.Level self, Player.IntroTypes playerIntro, bool isFromLoader)
    {
        orig(self, playerIntro, isFromLoader);

        currentRoomHeartGemToIndex.Clear();
        int i = 0;
        foreach (HeartGem heart in self.Tracker.GetEntities<HeartGem>())
        {
            currentRoomHeartGemToIndex[heart] = i++;
        }
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
                if (controller == null || controller.Dialogs.Count == 0)
                {
                    return origDialog;
                }

                HeartGem collectedHeartGem = currentRoomHeartGemToIndex.Keys.ToList().Find(gem => gem != null && gem.collected);
                int i = currentRoomHeartGemToIndex[collectedHeartGem];
                currentRoomHeartGemToIndex.Remove(collectedHeartGem);
                i = Math.Min(controller.Dialogs.Count - 1, i);
                return Dialog.Clean(controller.Dialogs[i]);
            });
            cursor.EmitStloc(14);
        }
    }
}