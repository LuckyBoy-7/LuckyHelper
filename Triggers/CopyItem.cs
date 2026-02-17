using System.Text.RegularExpressions;
using Celeste.Mod.Entities;
using LuckyHelper.Entities.Misc;
using LuckyHelper.Entities.Room;
using LuckyHelper.Extensions;
using LuckyHelper.Module;
using LuckyHelper.Modules;

namespace LuckyHelper.Triggers;

[CustomEntity("LuckyHelper/CopyItem")]
public class CopyItem : Trigger
{
    public CopyItem(EntityData data, Vector2 offset) : base(data, offset)
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

    public class CopiedItemData
    {
        public List<DataWithOffset<EntityData>> Entities;
        public List<DataWithOffset<EntityData>> Triggers;
        public List<DataWithOffset<DecalData>> FgDecals;
        public List<DataWithOffset<DecalData>> BgDecals;
    }

    public class DataWithOffset<T>
    {
        public T Data;
        public Vector2 PivotToItemOffset;
    }

    public static Dictionary<string, CopiedItemData> idToCopiedItemData = new();


    private static void EventsOnOnMapDataLoad(MapData mapData)
    {
        idToCopiedItemData.Clear();
        foreach (var levelData in mapData.Levels)
        {
            foreach (var entityData in levelData.Triggers)
            {
                if (entityData.Name == "LuckyHelper/CopyItem")
                    RegisterCopiedItems(entityData, levelData);
            }
        }
    }

    private static void RegisterCopiedItems(EntityData copyItemData, LevelData levelData)
    {
        string copiedToID = copyItemData.Attr("copiedToID");


        CopiedItemData data = new CopiedItemData();
        data.Entities = SelectInTriggerItems(copyItemData, levelData.Entities);
        data.Triggers = SelectInTriggerItems(copyItemData, levelData.Triggers);
        data.FgDecals = SelectInTriggerItems(copyItemData, levelData.FgDecals);
        data.BgDecals = SelectInTriggerItems(copyItemData, levelData.BgDecals);

        idToCopiedItemData[copiedToID] = data;
    }

    private static List<DataWithOffset<T>> SelectInTriggerItems<T>(EntityData triggerData, List<T> otherDatas)
    {
        float x = triggerData.Position.X;
        float y = triggerData.Position.Y;
        float width = triggerData.Width;
        float height = triggerData.Height;

        Vector2 nodePivot = triggerData.Nodes.Length > 0 ? triggerData.Nodes[0] : triggerData.Center();
        Vector2 pivotToTriggerOffset = triggerData.Position - nodePivot;

        List<DataWithOffset<T>> result = new();
        foreach (var data in otherDatas)
        {
            if (data is EntityData entityData && entityData == triggerData)
                continue;
            
            ParseItemBounds(data, out var otherX, out var otherY, out var otherWidth, out var otherHeight);

            if (otherX < x + width && otherX + otherWidth > x && otherY < y + height && otherY + otherHeight > y)
            {
                result.Add(new DataWithOffset<T>()
                {
                    Data = data,
                    PivotToItemOffset = new Vector2(otherX - x, otherY - y) + pivotToTriggerOffset
                });
            }
        }

        return result;
    }

    private static void ParseItemBounds<T>(T data, out float otherX, out float otherY, out float otherWidth, out float otherHeight)
    {
        otherX = 0;
        otherY = 0;
        otherWidth = 0;
        otherHeight = 0;
        if (data is EntityData entityData)
        {
            otherX = entityData.Position.X;
            otherY = entityData.Position.Y;
            otherWidth = entityData.Width;
            otherHeight = entityData.Height;
        }
        else if (data is DecalData decalData)
        {
            otherX = decalData.Position.X;
            otherY = decalData.Position.Y;
            string extension = Path.GetExtension(decalData.Texture);
            string text = Path.Combine("decals", decalData.Texture.Replace(extension, "")).Replace('\\', '/');
            var textures = GFX.Game.GetAtlasSubtextures(Regex.Replace(text, "\\d+$", string.Empty));
            if (textures.Count > 0)
            {
                otherWidth = textures[0].Width;
                otherHeight = textures[0].Height;
            }
        }
    }
}