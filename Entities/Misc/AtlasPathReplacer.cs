using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using Celeste.Mod.Entities;
using LuckyHelper.Module;
using LuckyHelper.Modules;
using LuckyHelper.Utils;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace LuckyHelper.Entities.Misc;

public enum AtlasTypes
{
    Game,
    Gui,
    Opening,
    Misc,
    Portraits,
    ColorGrades,
    Others
}

class AtlasPathReplacerHelper
{
    class Group
    {
        private Dictionary<string, string> pairs;
        private SimpleValidater simpleRoomValidater;

        public Group(Dictionary<string, string> pairs, SimpleValidater simpleRoomValidater)
        {
            this.pairs = pairs;
            this.simpleRoomValidater = simpleRoomValidater;
        }

        public bool IsValidRoom(string roomName) => simpleRoomValidater != null && simpleRoomValidater.Valid(roomName);
        public bool Contains(string from) => pairs != null && pairs.ContainsKey(from);

        public bool TryGetMapping(string from, out string to)
        {
            to = null;
            return pairs != null && pairs.TryGetValue(from, out to);
        }
    }

    private Dictionary<AtlasTypes, List<Group>> atlasTypeToGroup = new();

    // 因为很多时候一整张图可能也就几条路径被替换，提前特判可能在多 group 场景下会稍微省点性能
    private HashSet<string> affectedPath = new();

    public void Register(EntityData entityData)
    {
        AtlasTypes atlasType = entityData.Enum<AtlasTypes>("atlasType", AtlasTypes.Game);
        List<string> from = entityData.ParseToStringList("from");
        List<string> to = entityData.ParseToStringList("to");
        List<string> rooms = entityData.ParseToStringList("rooms");


        // pairs
        Dictionary<string, string> pairs = new();
        int n = Math.Min(from.Count, to.Count);
        for (int i = 0; i < n; i++)
        {
            pairs[from[i]] = to[i];
            affectedPath.Add(from[i]);
        }

        // room validater
        SimpleValidater simpleValidater = new(rooms);
        Group group = new(pairs, simpleValidater);
        if (!atlasTypeToGroup.ContainsKey(atlasType))
            atlasTypeToGroup[atlasType] = new List<Group>();
        atlasTypeToGroup[atlasType].Add(group);
    }

    public bool IsValidRoom(AtlasTypes atlasTypes, string from, string roomName)
    {
        if (atlasTypeToGroup.TryGetValue(atlasTypes, out List<Group> groups))
        {
            foreach (Group group in groups)
            {
                if (group.Contains(from) && group.IsValidRoom(roomName))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool TryGetMapping(AtlasTypes atlasTypes, string from, out string to)
    {
        to = null;
        if (!affectedPath.Contains(from))
            return false;
        if (atlasTypeToGroup.TryGetValue(atlasTypes, out List<Group> groups))
        {
            foreach (Group group in groups)
            {
                if (group.TryGetMapping(from, out to))
                {
                    return true;
                }
            }
        }

        return false;
    }
}

[Tracked]
[CustomEntity("LuckyHelper/AtlasPathReplacer")]
public class AtlasPathReplacer : Entity
{
    public AtlasPathReplacer(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
    }

    private static Hook atlasGetItemHook;

    [Load]
    public static void Load()
    {
        DisableInlining();

        Events.OnMapDataLoad += EventsOnOnMapDataLoad;

        On.Monocle.Atlas.GetAtlasSubtextures += AtlasOnGetAtlasSubtextures;
        On.Monocle.Atlas.GetAtlasSubtextureFromAtlasAt += AtlasOnGetAtlasSubtextureFromAtlasAt;


        // Atlas this[string id]
        MethodInfo getItemMethod = typeof(Atlas).GetMethod(
            "get_Item",
            BindingFlags.Public | BindingFlags.Instance
        );
        if (getItemMethod == null)
        {
            LogUtils.LogWarning($"Could not find method this in {nameof(SpriteBatch)}!");
            return;
        }

        atlasGetItemHook = new Hook(getItemMethod, AtlasGetItemHook);
    }


    [Unload]
    public static void Unload()
    {
        Events.OnMapDataLoad -= EventsOnOnMapDataLoad;

        On.Monocle.Atlas.GetAtlasSubtextures -= AtlasOnGetAtlasSubtextures;
        On.Monocle.Atlas.GetAtlasSubtextureFromAtlasAt -= AtlasOnGetAtlasSubtextureFromAtlasAt;

        atlasGetItemHook?.Dispose();
        atlasGetItemHook = null;
    }

    private static void EventsOnOnMapDataLoad(MapData mapData)
    {
        var dd = DynamicData.For(mapData);
        AtlasPathReplacerHelper atlasPathReplacerHelper = new();
        dd.Set("LuckyHelper_AtlasPathReplacer_atlasPathReplacerHelper", atlasPathReplacerHelper);

        foreach (var levelData in mapData.Levels)
        {
            foreach (var entityData in levelData.Entities)
            {
                if (entityData.Name == "LuckyHelper/AtlasPathReplacer")
                {
                    atlasPathReplacerHelper.Register(entityData);
                }
            }
        }
    }

    private static MTexture AtlasOnGetAtlasSubtextureFromAtlasAt(On.Monocle.Atlas.orig_GetAtlasSubtextureFromAtlasAt orig, Atlas self, string key, int index)
    {
        return orig(self, HandledPath(self, key), index);
    }

    private static List<MTexture> AtlasOnGetAtlasSubtextures(On.Monocle.Atlas.orig_GetAtlasSubtextures orig, Atlas self, string key)
    {
        return orig(self, HandledPath(self, key));
    }

    private static MTexture AtlasGetItemHook(Func<Atlas, string, MTexture> orig, Atlas self, string path)
    {
        return orig(self, HandledPath(self, path));
    }

    private static string HandledPath(Atlas atlas, string path)
    {
        if (MiscUtils.TryGetSession(out Session session))
        {
            var dd = DynamicData.For(session.MapData);
            if (dd.TryGet<AtlasPathReplacerHelper>("LuckyHelper_AtlasPathReplacer_atlasPathReplacerHelper", out var atlasPathReplacerHelper))
            {
                AtlasTypes atlasToType = AtlasToType(atlas);
                if (atlasPathReplacerHelper.TryGetMapping(atlasToType, path, out string newPath)
                    && atlasPathReplacerHelper.IsValidRoom(atlasToType, path, session.Level))
                {
                    return newPath;
                }
            }
        }

        return path;
    }

    private static AtlasTypes AtlasToType(Atlas atlas)
    {
        if (atlas == GFX.Game)
            return AtlasTypes.Game;
        if (atlas == GFX.Gui)
            return AtlasTypes.Gui;
        if (atlas == GFX.Opening)
            return AtlasTypes.Opening;
        if (atlas == GFX.Misc)
            return AtlasTypes.Misc;
        if (atlas == GFX.Portraits)
            return AtlasTypes.Portraits;
        if (atlas == GFX.ColorGrades)
            return AtlasTypes.ColorGrades;
        return AtlasTypes.Others;
    }


    private static void DisableInlining()
    {
        // this[string id]
        MethodInfo getItemMethod = typeof(Atlas).GetMethod(
            "get_Item",
            BindingFlags.Public | BindingFlags.Instance
        );
        if (getItemMethod == null)
            LogUtils.LogWarning($"Could not find method this in {nameof(Atlas)}!");
        else
            MonoMod.Core.Platforms.PlatformTriple.Current.TryDisableInlining(getItemMethod);

        // GetAtlasSubtexturesAt
        getItemMethod = typeof(Atlas).GetMethod(
            "GetAtlasSubtexturesAt",
            BindingFlags.Public | BindingFlags.Instance
        );
        if (getItemMethod == null)
            LogUtils.LogWarning($"Could not find method this in {nameof(Atlas)}!");
        else
            MonoMod.Core.Platforms.PlatformTriple.Current.TryDisableInlining(getItemMethod);
    }
}