using System.Collections;
using LuckyHelper.Utils;
using MonoMod.RuntimeDetour;
using Platform = On.Celeste.Platform;
using Player = On.Celeste.Player;

namespace LuckyHelper.Module;

public class LuckyHelperModule : EverestModule
{
    public static LuckyHelperModule Instance { get; private set; }

    public override Type SettingsType => typeof(LuckyHelperSettings);
    public static LuckyHelperSettings Settings => (LuckyHelperSettings)Instance._Settings;

    public override Type SessionType => typeof(LuckyHelperSession);
    public static LuckyHelperSession Session => (LuckyHelperSession)Instance._Session;

    public override Type SaveDataType => typeof(LuckyHelperSaveData);
    public static LuckyHelperSaveData SaveData => (LuckyHelperSaveData)Instance._SaveData;

    public LuckyHelperModule()
    {
        Instance = this;
        AttributeUtils.CollectMethods<LoadAttribute>();
        AttributeUtils.CollectMethods<UnloadAttribute>();
        AttributeUtils.CollectMethods<InitializeAttribute>();
    }


    public override void Initialize()
    {
        AttributeUtils.Invoke<InitializeAttribute>();
    }

    public override void Load()
    {
        // Logger.Log(LogLevel.Info, "Test", "alskdjf;alsdkjf");
        AttributeUtils.Invoke<LoadAttribute>();
    }

    public override void Unload()
    {
        AttributeUtils.Invoke<UnloadAttribute>();
    }
}