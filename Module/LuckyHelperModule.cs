using LuckyHelper.Entities.Misc;
using LuckyHelper.Entities.Room;
using LuckyHelper.Handlers;
using LuckyHelper.Handlers.Impl;
using LuckyHelper.ModInterop;
using LuckyHelper.Modules;
using LuckyHelper.Utils;
using MonoMod.ModInterop;

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

        // https://discord.com/channels/403698615446536203/908809001834274887/1418153890020589668
        // 有点不理解, 所以暂时先这么写了(看了源码感觉没问题, 但是实际上用 GetComponentsTrackIfNeeded 的时候可能触发 failed for an unknown reason
        if (!Tracker.StoredComponentTypes.Contains(typeof(TalkComponent)))
        {
            Tracker.AddTypeToTracker(typeof(TalkComponent));
        }

        if (!Tracker.StoredEntityTypes.Contains(typeof(TextMenu)))
        {
            Tracker.AddTypeToTracker(typeof(TextMenu));
        }
    }

    public override void Load()
    {
        ModCompatModule.Load();
        AttributeUtils.Invoke<LoadAttribute>();

        PasteRoom.Load();


        // eevee: https://github.com/CommunalHelper/EeveeHelper/blob/dev/Code/EeveeHelperModule.cs#L48
        EntityHandler.RegisterInherited<Water>((entity, container) => new WaterHandler(entity));
        EntityHandler.RegisterInherited<TrackSpinner>((entity, container) => new TrackSpinnerHandler(entity));
        EntityHandler.RegisterInherited<RotateSpinner>((entity, container) => new RotateSpinnerHandler(entity));
        EntityHandler.RegisterInherited<DreamBlock>((entity, container) => new DreamBlockHandler(entity));
        EntityHandler.RegisterInherited<TriggerSpikes>((entity, container) => new TriggerSpikesHandler(entity));

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

        typeof(LuckyHelperExports).ModInterop();
    }


    public override void LoadContent(bool firstLoad)
    {
        base.LoadContent(firstLoad);
        LuckyHelperEffects.LoadContent();
    }

    public override void Unload()
    {
        ModCompatModule.Unload();
        AttributeUtils.Invoke<UnloadAttribute>();

        PasteRoom.Unload();
    }
}