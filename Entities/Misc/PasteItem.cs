using Celeste.Mod.Entities;
using Lucky.Kits.Collections;
using Lucky.Kits.Extensions;
using LuckyHelper.Extensions;
using LuckyHelper.Module;
using LuckyHelper.Modules;
using LuckyHelper.Triggers;
using LuckyHelper.Utils;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace LuckyHelper.Entities.Misc;

[CustomEntity("LuckyHelper/PasteItem")]
public class PasteItem : Entity
{
    public string PastedFromID;
    public bool pasteEntity;
    public bool pasteTrigger;
    public bool pasteForegroundDecal;
    public bool pasteBackgroundDecal;

    public Vector2 PositionRelativeToRoom;


    public PasteItem(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        PositionRelativeToRoom = data.Position;
        PastedFromID = data.Attr("pastedFromID");
        pasteEntity = data.Bool("pasteEntity");
        pasteTrigger = data.Bool("pasteTrigger");
        pasteForegroundDecal = data.Bool("pasteForegroundDecal");
        pasteBackgroundDecal = data.Bool("pasteBackgroundDecal");
    }

    public static ILHook origLevelLoadHook;

    public static void Load()
    {
        origLevelLoadHook = new ILHook(typeof(Level).GetMethod("orig_LoadLevel"), LevelOnOirgLoadLevelILHook);
    }


    public static void Unload()
    {
        origLevelLoadHook?.Dispose();
        origLevelLoadHook = null;
    }


    // 本来可以提前记录省点的, 但后来发现好像
    public static List<PasteItem> currentPasteItems = new();


    private static void LevelOnOirgLoadLevelILHook(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);

        Type levelDataType = typeof(LevelData);
        if (cursor.TryGotoNext(ins => ins.MatchLdfld(levelDataType.GetField("Entities"))
            ))
        {
            cursor.Index += 1;

            cursor.EmitDup();
            cursor.EmitDelegate(InitializeCurrentPasteItems);

            cursor.EmitLdloc3();
            cursor.EmitDelegate(InsertCopiedEntities);
        }

        if (cursor.TryGotoNext(ins => ins.MatchLdfld(levelDataType.GetField("Triggers"))
            ))
        {
            cursor.Index += 1;

            cursor.EmitLdloc3();
            cursor.EmitDelegate(InsertCopiedTriggers);
        }

        if (cursor.TryGotoNext(ins => ins.MatchLdfld(levelDataType.GetField("FgDecals"))
            ))
        {
            cursor.Index += 1;

            cursor.EmitLdloc3();
            cursor.EmitDelegate(InsertCopiedFgDecals);
        }

        if (cursor.TryGotoNext(ins => ins.MatchLdfld(levelDataType.GetField("BgDecals"))
            ))
        {
            cursor.Index += 1;

            cursor.EmitLdloc3();
            cursor.EmitDelegate(InsertCopiedBgDecals);
        }
    }

    public static void InitializeCurrentPasteItems(List<EntityData> entities)
    {
        currentPasteItems = entities.Where(entityData => entityData.Name == "LuckyHelper/PasteItem")
            .Select(data => new PasteItem(data, Vector2.Zero)).ToList();
    }


    private static List<T> AddCopiedItems<T>(LevelData levelData, List<T> items, Func<PasteItem, bool> condition,
        Func<CopyItem.CopiedItemData, List<CopyItem.DataWithOffset<T>>> selectItem)
    {
        if (currentPasteItems.Count == 0)
            return items;
        
        List<T> newItems = new();
        foreach (PasteItem pasteItem in currentPasteItems)
        {
            if (condition(pasteItem) && CopyItem.idToCopiedItemData.TryGetValue(pasteItem.PastedFromID, out CopyItem.CopiedItemData copiedData))
            {
                InsertItems(newItems, pasteItem, selectItem);
            }
        }

        if (newItems.Count == 0)
            return items;

        List<T> returnItems = new(items);
        returnItems.AddRange(newItems);
        return returnItems;
    }

    private static void InsertItems<T>(List<T> items, PasteItem pasteItem, Func<CopyItem.CopiedItemData, List<CopyItem.DataWithOffset<T>>> selectItem)
    {
        if (!CopyItem.idToCopiedItemData.TryGetValue(pasteItem.PastedFromID, out CopyItem.CopiedItemData copiedData))
            return;
        List<CopyItem.DataWithOffset<T>> selectItems = selectItem(copiedData);

        if (typeof(T) != typeof(EntityData))
        {
            pasteItem.ApplyOffset(selectItems);
            items.AddRange(selectItems.Select(dataWithOffset => dataWithOffset.Data));
            return;
        }

        var (pasteDatas, normalDatas) = selectItems.Partition(dataWithOffset =>
        {
            if (dataWithOffset.Data is EntityData entityData)
            {
                return entityData.Name == "LuckyHelper/PasteItem";
            }

            return false;
        });

        // 同时 offset 了 PasteItem, 方便后边 PasteItem offset 其他的 item
        pasteItem.ApplyOffset(selectItems);
        items.AddRange(normalDatas.Select(dataWithOffset => dataWithOffset.Data));

        foreach (var pasteData in pasteDatas)
        {
            PasteItem paste = new PasteItem(pasteData.Data as EntityData, pasteData.PivotToItemOffset);
            InsertItems(items, paste, selectItem);
        }
    }

    private void ApplyOffset<T>(List<CopyItem.DataWithOffset<T>> datas)
    {
        foreach (var dataWithOffset in datas)
        {
            Vector2 offsetPosition = dataWithOffset.PivotToItemOffset + PositionRelativeToRoom;
            if (dataWithOffset.Data is EntityData entityData)
            {
                entityData.Position = offsetPosition;
            }
            else if (dataWithOffset.Data is DecalData decalData)
            {
                decalData.Position = offsetPosition;
            }
        }
    }


    private static List<EntityData> InsertCopiedEntities(List<EntityData> entityDatas, LevelData levelData) =>
        AddCopiedItems(levelData, entityDatas, pasteItem => pasteItem.pasteEntity, data => data.Entities.Select(GetCopiedEntityData).ToList());

    private static List<EntityData> InsertCopiedTriggers(List<EntityData> entityDatas, LevelData levelData) =>
        AddCopiedItems(levelData, entityDatas, pasteItem => pasteItem.pasteTrigger, data => data.Triggers.Select(GetCopiedEntityData).ToList());

    private static List<DecalData> InsertCopiedFgDecals(List<DecalData> entityDatas, LevelData levelData) =>
        AddCopiedItems(levelData, entityDatas, pasteItem => pasteItem.pasteForegroundDecal, data => data.FgDecals.Select(GetCopiedDecalData).ToList());

    private static List<DecalData> InsertCopiedBgDecals(List<DecalData> entityDatas, LevelData levelData) =>
        AddCopiedItems(levelData, entityDatas, pasteItem => pasteItem.pasteBackgroundDecal, data => data.BgDecals.Select(GetCopiedDecalData).ToList());

    private static CopyItem.DataWithOffset<EntityData> GetCopiedEntityData(CopyItem.DataWithOffset<EntityData> orig)
    {
        var newData = new CopyItem.DataWithOffset<EntityData>()
        {
            Data = orig.Data.Clone(),
            PivotToItemOffset = orig.PivotToItemOffset
        };

        return newData;
    }

    private static CopyItem.DataWithOffset<DecalData> GetCopiedDecalData(CopyItem.DataWithOffset<DecalData> orig)
    {
        var newData = new CopyItem.DataWithOffset<DecalData>()
        {
            Data = orig.Data.Clone(),
            PivotToItemOffset = orig.PivotToItemOffset
        };

        return newData;
    }
}