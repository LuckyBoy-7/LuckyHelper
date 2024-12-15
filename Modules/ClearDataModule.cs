using LuckyHelper.Extensions;
using LuckyHelper.Module;
using LuckyHelper.Triggers;
using Player = On.Celeste.Player;
using Session = On.Celeste.Session;

namespace LuckyHelper.Modules;

public class ClearDataModule
{


    [Load]
    public static void Load()
    {
        On.Celeste.Session.ctor += SessionOnctor;
    }



    [Unload]
    public static void Unload()
    {
        On.Celeste.Session.ctor -= SessionOnctor;
    }
    
    
    private static void SessionOnctor(Session.orig_ctor orig, Celeste.Session self)
    {
        orig(self);
        ResetData();
    }

    private static void ResetData()
    {
        LuckyHelperModule.SaveData.PlayerLastCheckPoint = "StartCheckpoint";
    }
}