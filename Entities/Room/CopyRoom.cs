using Celeste.Mod.Entities;
using LuckyHelper.Extensions;
using LuckyHelper.Module;
using LuckyHelper.Modules;

namespace LuckyHelper.Entities.Room;

[CustomEntity("LuckyHelper/CopyRoom")]
public class CopyRoom : Entity
{
    public CopyRoom(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
    }

    [Load]
    public static void Load()
    {
        Events.OnMapDataLoad += EventsOnOnMapDataLoad;
    }

    [Unload]
    public static void Unload()
    {
        Events.OnMapDataLoad -= EventsOnOnMapDataLoad;
    }

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
    

    private static void EventsOnOnMapDataLoad(MapData mapData)
    {
        levelToCopiedLevelData.Clear();
        levelToPasteRooms.Clear();
        foreach (var levelData in mapData.Levels)
        {
            foreach (var entityData in levelData.Entities)
            {
                if (entityData.Name == "LuckyHelper/CopyRoom")
                {
                    string copiedToID = entityData.Attr("copiedToID");

                    CopiedLevelData data = new CopiedLevelData();
                    data.Entities = levelData.Entities;
                    data.Triggers = levelData.Triggers;
                    data.FgDecals = levelData.FgDecals;
                    data.BgDecals = levelData.BgDecals;
                    data.Solids = levelData.Solids;
                    data.Bg = levelData.Bg;

                    levelToCopiedLevelData[copiedToID] = data;
                }
                else if (entityData.Name == "LuckyHelper/PasteRoom")
                {
                    if (!levelToPasteRooms.ContainsKey(levelData))
                        levelToPasteRooms[levelData] = new List<PasteRoom>();
                    levelToPasteRooms[levelData].Add(new PasteRoom(entityData, Vector2.Zero));
                }
            }
        }

        foreach (var (_, pasteRooms) in levelToPasteRooms)
        {
            pasteRooms.Sort((left, right) => left.PasteOrder - right.PasteOrder);
        }
    }
}