using Lucky.Kits.Collections;

namespace LuckyHelper.Module;

public class LuckyHelperSession : EverestModuleSession
{
    // misc
    public Dictionary<string, int> RoomIdToPassByRefillDahes = new();

    // deathCount
    public int TotalDeathCount = 0;
    public DefaultDict<string, int> CurrentRoomDeathCount = new(() => 0);
    public DefaultDict<string, int> SavedPathDeathCount = new(() => 0);
    public DefaultDict<string, int> CurrentCheckpointDeathCount = new(() => 0);
    
    // timer
    public float TotalTime = 0;
    public DefaultDict<string, float> CurrentRoomTime = new(() => 0);
    public DefaultDict<string, float> SavedPathTime = new(() => 0);
    public DefaultDict<string, float> CurrentCheckpointTime = new(() => 0);
    
    // checkpoint
    public string PlayerLastCheckPoint = "StartCheckpoint";

    // custom water
    public float KillPlayerElapse = 0;

}