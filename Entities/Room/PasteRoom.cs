using System.Text.RegularExpressions;
using Celeste.Mod.Entities;
using LuckyHelper.Extensions;
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
    public class CopiedLevelData
    {
        public List<EntityData> Entities;
        public List<EntityData> Triggers;
        public List<DecalData> FgDecals;
        public List<DecalData> BgDecals;
        public string Solids;
        public string Bg;
    }

    public static Dictionary<string, CopiedLevelData> levelToCopiedLevelData = new();
    public static Dictionary<LevelData, List<PasteRoom>> levelToPasteRooms = new();


    public string PastedFromRoom;
    public int PasteOrder;
    public float OffsetX;
    public float OffsetY;
    private bool pasteEntity;
    private bool pasteTrigger;
    public bool pasteForegroundDecal;
    public bool pasteBackgroundDecal;
    public bool pasteForegroundTile;
    public bool pasteBackgroundTile;


    public PasteRoom(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        PastedFromRoom = data.Attr("pastedFromRoom");
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

    public static void Load()
    {
        origLevelLoadHook = new ILHook(typeof(Level).GetMethod("orig_LoadLevel"), LevelOnOirgLoadLevelILHook);
        IL.Celeste.LevelLoader.LoadingThread += LevelLoaderOnLoadingThread;
        Events.OnMapDataLoad += EventsOnOnMapDataLoad;
    }


    public static void Unload()
    {
        origLevelLoadHook?.Dispose();
        origLevelLoadHook = null;

        IL.Celeste.LevelLoader.LoadingThread -= LevelLoaderOnLoadingThread;
        Events.OnMapDataLoad -= EventsOnOnMapDataLoad;
    }


    private static void EventsOnOnMapDataLoad(MapData mapData)
    {
        levelToCopiedLevelData.Clear();
        levelToPasteRooms.Clear();

        HashSet<string> shouldCopiedLevels = new();
        foreach (var levelData in mapData.Levels)
        {
            foreach (var entityData in levelData.Entities)
            {
                if (entityData.Name != "LuckyHelper/PasteRoom")
                    continue;

                if (!levelToPasteRooms.ContainsKey(levelData))
                    levelToPasteRooms[levelData] = new List<PasteRoom>();
                PasteRoom pasteRoom = new PasteRoom(entityData, Vector2.Zero);
                levelToPasteRooms[levelData].Add(pasteRoom);
                shouldCopiedLevels.Add(pasteRoom.PastedFromRoom);
            }
        }

        foreach (var (_, pasteRooms) in levelToPasteRooms)
        {
            pasteRooms.Sort((left, right) => left.PasteOrder - right.PasteOrder);
        }

        foreach (var levelData in mapData.Levels)
        {
            if (!shouldCopiedLevels.Contains(levelData.Name))
                continue;
            CopiedLevelData data = new CopiedLevelData
            {
                Entities = levelData.Entities,
                Triggers = levelData.Triggers,
                FgDecals = levelData.FgDecals,
                BgDecals = levelData.BgDecals,
                Solids = levelData.Solids,
                Bg = levelData.Bg
            };

            levelToCopiedLevelData[levelData.Name] = data;
        }
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

        foreach (var (levelData, pasteRooms) in levelToPasteRooms)
        {
            foreach (var pasteRoom in pasteRooms)
            {
                if (!levelToCopiedLevelData.TryGetValue(pasteRoom.PastedFromRoom, out CopiedLevelData copiedData))
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


    private static List<T> InsertItems<T>(LevelData levelData, List<T> items, Func<PasteRoom, bool> condition, Func<CopiedLevelData, List<T>> selectItem)
    {
        if (!levelToPasteRooms.TryGetValue(levelData, out List<PasteRoom> pasteRooms))
            return items;
        List<T> newItems = new();
        foreach (PasteRoom pasteRoom in pasteRooms)
        {
            if (condition(pasteRoom) && levelToCopiedLevelData.TryGetValue(pasteRoom.PastedFromRoom, out CopiedLevelData copiedData))
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
        InsertItems(levelData, entityDatas, pasteRoom => pasteRoom.pasteEntity, data => data.Entities.Select(EntityDataExtensions.Clone).ToList());

    private static List<EntityData> InsertCopiedTriggers(List<EntityData> entityDatas, LevelData levelData) =>
        InsertItems(levelData, entityDatas, pasteRoom => pasteRoom.pasteTrigger, data => data.Triggers.Select(EntityDataExtensions.Clone).ToList());

    private static List<DecalData> InsertCopiedFgDecals(List<DecalData> entityDatas, LevelData levelData) =>
        InsertItems(levelData, entityDatas, pasteRoom => pasteRoom.pasteForegroundDecal, data => data.FgDecals.Select(DecalDataExtensions.Clone).ToList());

    private static List<DecalData> InsertCopiedBgDecals(List<DecalData> entityDatas, LevelData levelData) =>
        InsertItems(levelData, entityDatas, pasteRoom => pasteRoom.pasteBackgroundDecal, data => data.BgDecals.Select(DecalDataExtensions.Clone).ToList());


}