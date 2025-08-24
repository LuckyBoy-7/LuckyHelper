using LuckyHelper.Handlers;
using LuckyHelper.Handlers.Impl;
using LuckyHelper.Modules;
using LuckyHelper.Utils;

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
        ModCompatModule.Load();
        AttributeUtils.Invoke<LoadAttribute>();


        // eevee: https://github.com/CommunalHelper/EeveeHelper/blob/dev/Code/EeveeHelperModule.cs#L48
        EntityHandler.RegisterInherited<Water>((entity, container) => new WaterHandler(entity));
        EntityHandler.RegisterInherited<TrackSpinner>((entity, container) => new TrackSpinnerHandler(entity));
        EntityHandler.RegisterInherited<RotateSpinner>((entity, container) => new RotateSpinnerHandler(entity));
        EntityHandler.RegisterInherited<DreamBlock>((entity, container) => new DreamBlockHandler(entity));

        EntityHandler.RegisterInherited<DashSwitch>((entity, container) => new AxisMoverHandler(entity, new Tuple<string, bool>("startY", true)));

        EntityHandler.RegisterInherited<ZipMover>((entity, container) => new ZipMoverNodeHandler(entity, true),
            (entity, container) => ZipMoverNodeHandler.InsideCheck(container, true, entity as ZipMover));
        EntityHandler.RegisterInherited<ZipMover>((entity, container) => new ZipMoverNodeHandler(entity, false),
            (entity, container) => ZipMoverNodeHandler.InsideCheck(container, false, entity as ZipMover));
        EntityHandler.RegisterInherited<SwapBlock>((entity, container) => new SwapBlockHandler(entity, true),
            (entity, container) => SwapBlockHandler.InsideCheck(container, true, entity as SwapBlock));
        EntityHandler.RegisterInherited<SwapBlock>((entity, container) => new SwapBlockHandler(entity, false),
            (entity, container) => SwapBlockHandler.InsideCheck(container, false, entity as SwapBlock));
        EntityHandler.RegisterInherited<Decal>((entity, container) => new DecalHandler(entity),
            (entity, container) => container.CheckDecal(entity as Decal));

    }

    public override void Unload()
    {
        ModCompatModule.Unload();
        AttributeUtils.Invoke<UnloadAttribute>();
    }
}