using Celeste.Mod.Entities;
using LuckyHelper.Extensions;
using LuckyHelper.Module;
using LuckyHelper.Modules;
using LuckyHelper.Utils;
using MonoMod.Utils;

namespace LuckyHelper.Entities;

[CustomEntity("LuckyHelper/MenuButtonController")]
[Tracked]
public class MenuButtonController : Entity
{
    [Load]
    public static void Load()
    {
        On.Celeste.Level.Update += LevelOnUpdate;
        On.Celeste.TextMenu.Item.ctor += ItemOnctor;
    }

    [Unload]
    public static void Unload()
    {
        On.Celeste.Level.Update -= LevelOnUpdate;
    }

    private static void ItemOnctor(On.Celeste.TextMenu.Item.orig_ctor orig, TextMenu.Item self)
    {
        if (Engine.Scene is Level level && level.Tracker.GetEntities<MenuButtonController>().Count > 0)
            new DynamicData(self).Set(RawDialogNameToken, MiscModule.LastCleanedDialog);
        orig(self);
    }


    private static void LevelOnUpdate(On.Celeste.Level.orig_Update orig, Celeste.Level self)
    {
        orig(self);
        if (self.Tracker.GetEntity<MenuButtonController>() is { } buttonController)
        {
            foreach (TextMenu.Item item in self.Tracker.GetEntitiesTrackIfNeeded(typeof(TextMenu)).Cast<TextMenu>().SelectMany(menu => menu.items))
            {
                if (item is TextMenu.Button button)
                {
                    if (new DynamicData(button).TryGet(RawDialogNameToken, out string rawDialogName))
                    {
                        item.Disabled = !buttonController.GetButtonState(rawDialogName, !item.Disabled);
                    }
                }
            }
        }
    }


    public Dictionary<string, string> ButtonNamesToDisableFlags = new();

    public const string RawDialogNameToken = "LuckyHelper_RawDialogName";


    public MenuButtonController(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        List<string> ButtonNames = data.ParseToStringList("buttonNames");
        List<string> DisabledIfFlags = data.ParseToStringList("disabledIfFlags");
        int n = Math.Min(ButtonNames.Count, DisabledIfFlags.Count);
        for (int i = 0; i < n; i++)
        {
            ButtonNamesToDisableFlags[ButtonNames[i]] = DisabledIfFlags[i];
        }
    }


    private bool GetButtonState(string rawDialogName, bool origState)
    {
        if (!ButtonNamesToDisableFlags.TryGetValue(rawDialogName, out var flag))
            return origState;
        return !this.Session().GetFlag(flag);
    }
}