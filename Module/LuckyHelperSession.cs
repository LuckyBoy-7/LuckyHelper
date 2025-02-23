namespace LuckyHelper.Module;

public class LuckyHelperSession : EverestModuleSession
{
    // misc
    public Dictionary<string, int> RoomIdToPassByRefillDahes = new();

    // Falling Block blocks Floaty Space Block
    public bool EnableFallingBlockBlocksFloatySpaceBlock = false;

    // deathCount
    public int TotalDeathCount = 0;
    public Dictionary<string, int> CurrentRoomDeathCount = new();
    public Dictionary<string, int> SavedPathDeathCount = new();
    public Dictionary<string, int> CurrentCheckpointDeathCount = new();

    // timer
    public float TotalTime = 0;
    public Dictionary<string, float> CurrentRoomTime = new();
    public Dictionary<string, float> SavedPathTime = new();
    public Dictionary<string, float> CurrentCheckpointTime = new();

    // checkpoint
    public string PlayerLastCheckPoint = "StartCheckpoint";

    // custom water
    public float KillPlayerElapse = 0;
    
    // bloompointAdjustTrigger
    public bool LightFactorOn = false;
    public bool AffectLightRadius = true;
    public bool AffectLightAlpha = true;
    public float LightFactor = 1;
    public List<string> LightFactorTargets = new();

}