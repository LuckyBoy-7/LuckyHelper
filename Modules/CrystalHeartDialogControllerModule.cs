using System.Reflection;
using LuckyHelper.Entities.Misc;
using LuckyHelper.Module;
using LuckyHelper.Utils;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using Level = On.Celeste.Level;

namespace LuckyHelper.Modules;

public class CrystalHeartDialogControllerModule
{
    private static ILHook heartGemCollectCoroutineHook;

    // 因为水晶之心被收集之后就不会再生成了, 所以只在进房间的时候加载一次也不会有问题
    private static Dictionary<int, int> CurrentRoomHeartGemIDToIndex => LuckyHelperModule.Session.CurrentRoomHeartGemIDToIndex;

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

        int i = 0;
        foreach (HeartGem heart in self.Tracker.GetEntities<HeartGem>())
        {
            int id = heart.SourceId.ID;
            if (!CurrentRoomHeartGemIDToIndex.ContainsKey(id))
                CurrentRoomHeartGemIDToIndex[id] = i++;
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
            // heart gem
            cursor.EmitLdloc1();
            cursor.EmitLdloc(14);
            cursor.EmitDelegate<Func<HeartGem, string, string>>((gem, origDialog) =>
            {
                Tracker tracker = Engine.Scene.Tracker;
                CrystalHeartDialogController controller = tracker.GetEntity<CrystalHeartDialogController>();
                if (controller == null || controller.Dialogs.Count == 0)
                {
                    return origDialog;
                }

                int id = gem.SourceId.ID;
                if (CurrentRoomHeartGemIDToIndex.TryGetValue(id, out int index))
                {
                    index = Math.Min(controller.Dialogs.Count - 1, index);
                    return Dialog.Clean(controller.Dialogs[index]);
                }

                return origDialog;
            });
            cursor.EmitStloc(14);
        }
    }
}