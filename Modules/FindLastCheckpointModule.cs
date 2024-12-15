using LuckyHelper.Extensions;
using LuckyHelper.Module;
using LuckyHelper.Triggers;
using Checkpoint = On.Celeste.Checkpoint;
using Player = On.Celeste.Player;

namespace LuckyHelper.Modules;

public class FindLastCheckpointModule
{
    [Load]
    public static void Load()
    {
        On.Celeste.Checkpoint.Added += CheckpointOnAdded;
    }

    [Unload]
    public static void Unload()
    {
        On.Celeste.Checkpoint.Added -= CheckpointOnAdded;
    }

    private static void CheckpointOnAdded(Checkpoint.orig_Added orig, Celeste.Checkpoint self, Scene scene)
    {
        orig(self, scene);
        LuckyHelperModule.SaveData.PlayerLastCheckPoint = self.GetCheckpointName();
    }
}