using System.Text.RegularExpressions;
using Celeste.Mod.Entities;
using LuckyHelper.Module;
using LuckyHelper.Modules;
using LuckyHelper.Utils;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using LevelLoader = On.Celeste.LevelLoader;

namespace LuckyHelper.Entities.Room;

[CustomEntity("LuckyHelper/PasteRoom")]
public class PasteRoom : Entity
{
    private string pastedFromID;
    public int PasteOrder;
    public float OffsetX;
    public float OffsetY;
    private bool pasteEntity;
    private bool pasteTrigger;
    private bool pasteForegroundDecal;
    private bool pasteBackgroundDecal;
    private bool pasteForegroundTile;
    private bool pasteBackgroundTile;


    public PasteRoom(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        pastedFromID = data.Attr("pastedFromID");
        PasteOrder = data.Int("pasteOrder");
        OffsetX = data.Float("offsetX");
        OffsetY = data.Float("offsetY");
        pasteEntity = data.Bool("pasteEntity");
        pasteTrigger = data.Bool("pasteTrigger");
        pasteForegroundDecal = data.Bool("pasteForegroundDecal");
        pasteBackgroundDecal = data.Bool("pasteBackgroundDecal");
        pasteForegroundTile = data.Bool("pasteForegroundTile");
        pasteBackgroundTile = data.Bool("pasteBackgroundTile");
    }

    public static ILHook origLevelLoadHook;

    [Load]
    public static void Load()
    {
        origLevelLoadHook = new ILHook(typeof(Level).GetMethod("orig_LoadLevel"), LevelOnOirgLoadLevelILHook);
        IL.Celeste.LevelLoader.LoadingThread += LevelLoaderOnLoadingThread;
    }


    [Unload]
    public static void Unload()
    {
        origLevelLoadHook?.Dispose();
        origLevelLoadHook = null;

        IL.Celeste.LevelLoader.LoadingThread -= LevelLoaderOnLoadingThread;
    }

    private static void LevelLoaderOnLoadingThread(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);

        if (cursor.TryGotoNext(
                ins => ins.MatchLdloc0(),
                ins => ins.MatchLdfld(typeof(MapData).GetField("Filler"))
            ))
        {
            cursor.Index += 1;
            cursor.EmitLdloc0();
            cursor.EmitLdloc(13);
            cursor.EmitLdloc(14);
            cursor.EmitDelegate(ReplaceFgBgTiles);
        }
    }

    private static void ReplaceFgBgTiles(MapData mapData, VirtualMap<char> bgVirtualMap, VirtualMap<char> fgVirtualMap)
    {
        Rectangle mapTileBounds = mapData.TileBounds;
        Regex regex = new Regex("\\r\\n|\\n\\r|\\n|\\r");

        foreach (var (levelData, pasteRooms) in CopyRoom.levelToPasteRooms)
        {
            foreach (var pasteRoom in pasteRooms)
            {
                if (!CopyRoom.levelToCopiedLevelData.TryGetValue(pasteRoom.pastedFromID, out CopyRoom.CopiedLevelData copiedData))
                    continue;

                int offsetX = (int)Math.Floor(pasteRoom.OffsetX / 8 + 0.5f);
                int offsetY = (int)Math.Floor(pasteRoom.OffsetY / 8 + 0.5f);

                if (pasteRoom.pasteBackgroundTile)
                {
                    // string[] origMatrix = regex.Split(levelData.Bg);
                    string[] copiedMatrix = regex.Split(copiedData.Bg);
                    int rows = copiedMatrix.Length;
                    int columns = copiedMatrix.Max(s => s.Length);

                    int levelLeft = levelData.TileBounds.Left;
                    int levelTop = levelData.TileBounds.Top;
                    for (int i = levelTop; i < levelTop + rows; i++)
                    {
                        for (int j = levelLeft; j < levelLeft + columns; j++)
                        {
                            var (nx, ny) = (j - levelLeft, i - levelTop);
                            if (ny < copiedMatrix.Length && ny >= 0 && nx < copiedMatrix[ny].Length && nx >= 0 && copiedMatrix[ny][nx] != '0')
                                bgVirtualMap[j - mapTileBounds.X + offsetX, i - mapTileBounds.Y + offsetY] = copiedMatrix[ny][nx];
                        }
                    }
                }


                if (pasteRoom.pasteForegroundTile)
                {
                    // string[] origMatrix = regex.Split(levelData.Solids);
                    string[] copiedMatrix = regex.Split(copiedData.Solids);
                    int columns = copiedMatrix.Max(s => s.Length);
                    int rows = copiedMatrix.Length;

                    int levelLeft = levelData.TileBounds.Left;
                    int levelTop = levelData.TileBounds.Top;
                    for (int i = levelTop; i < levelTop + rows; i++)
                    {
                        for (int j = levelLeft; j < levelLeft + columns; j++)
                        {
                            var (nx, ny) = (j - levelLeft, i - levelTop);
                            if (ny < copiedMatrix.Length && ny >= 0 && nx < copiedMatrix[ny].Length && nx >= 0 && copiedMatrix[ny][nx] != '0')
                                fgVirtualMap[j - mapTileBounds.X + offsetX, i - mapTileBounds.Y + offsetY] = copiedMatrix[ny][nx];
                        }
                    }
                }
            }
        }
    }

    private static void LevelOnOirgLoadLevelILHook(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);

        Type levelDataType = typeof(LevelData);
        if (cursor.TryGotoNext(ins => ins.MatchLdfld(levelDataType.GetField("Entities"))
            ))
        {
            cursor.Index += 1;

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


    private static List<T> InsertItems<T>(LevelData levelData, List<T> items, Func<PasteRoom, bool> condition, Func<CopyRoom.CopiedLevelData, List<T>> selectItem)
    {
        if (!CopyRoom.levelToPasteRooms.TryGetValue(levelData, out List<PasteRoom> pasteRooms))
            return items;
        List<T> newItems = new();
        foreach (PasteRoom pasteRoom in pasteRooms)
        {
            if (condition(pasteRoom) && CopyRoom.levelToCopiedLevelData.TryGetValue(pasteRoom.pastedFromID, out CopyRoom.CopiedLevelData copiedData))
            {
                List<T> selectItems = selectItem(copiedData);

                pasteRoom.ApplyOffset(selectItems);
                newItems.AddRange(selectItems);
            }
        }

        if (newItems.Count == 0)
            return items;

        List<T> returnItems = new(items);
        returnItems.AddRange(newItems);
        return returnItems;
    }

    private void ApplyOffset<T>(List<T> selectItems)
    {
        foreach (var item in selectItems)
        {
            if (item is EntityData entityData)
            {
                entityData.Position += new Vector2(OffsetX, OffsetY);
            }
            else if (item is DecalData decalData)
            {
                decalData.Position += new Vector2(OffsetX, OffsetY);
            }
        }
    }


    private static List<EntityData> InsertCopiedEntities(List<EntityData> entityDatas, LevelData levelData) =>
        InsertItems(levelData, entityDatas, pasteRoom => pasteRoom.pasteEntity, data => data.Entities.Select(GetCopiedEntityData).ToList());

    private static List<EntityData> InsertCopiedTriggers(List<EntityData> entityDatas, LevelData levelData) =>
        InsertItems(levelData, entityDatas, pasteRoom => pasteRoom.pasteTrigger, data => data.Triggers.Select(GetCopiedEntityData).ToList());

    private static List<DecalData> InsertCopiedFgDecals(List<DecalData> entityDatas, LevelData levelData) =>
        InsertItems(levelData, entityDatas, pasteRoom => pasteRoom.pasteForegroundDecal, data => data.FgDecals.Select(GetCopiedDecalData).ToList());

    private static List<DecalData> InsertCopiedBgDecals(List<DecalData> entityDatas, LevelData levelData) =>
        InsertItems(levelData, entityDatas, pasteRoom => pasteRoom.pasteBackgroundDecal, data => data.BgDecals.Select(GetCopiedDecalData).ToList());

    private static EntityData GetCopiedEntityData(EntityData orig)
    {
        var newData = new EntityData
        {
            Name = orig.Name,
            ID = orig.ID,
            Level = orig.Level, // 应用偏移
            Position = orig.Position, // 应用偏移
            Width = orig.Width,
            Height = orig.Height,
            Origin = orig.Origin,
            Nodes = orig.Nodes?.Select(node => node).ToArray()
        };
        if (orig.Values != null)
            newData.Values = new Dictionary<string, object>(orig.Values);

        return newData;
    }

    private static DecalData GetCopiedDecalData(DecalData orig)
    {
        var newData = new DecalData
        {
            Texture = orig.Texture,
            Position = orig.Position,
            Scale = orig.Scale, // 应用偏移
            Rotation = orig.Rotation, // 应用偏移
            ColorHex = orig.ColorHex,
            Depth = orig.Depth,
            Parallax = orig.Parallax,
        };

        return newData;
    }
}