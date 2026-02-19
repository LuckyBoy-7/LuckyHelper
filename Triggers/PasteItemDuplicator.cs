using System.Text.RegularExpressions;
using Celeste.Mod.Entities;
using LuckyHelper.Entities.Misc;
using LuckyHelper.Entities.Room;
using LuckyHelper.Extensions;
using LuckyHelper.Module;
using LuckyHelper.Modules;

namespace LuckyHelper.Triggers;

[CustomEntity("LuckyHelper/PasteItemDuplicator")]
public class PasteItemDuplicator : Trigger
{
    private Vector2 startOffset;
    private Vector2 placementOffset;
    private int pasteNumber;

    private bool duplicateOnEnterRoom;
    private bool showDuplicatedOrderNumber;
    private string duplicateFlag;

    public PasteItemDuplicator(EntityData data, Vector2 offset) : base(data, offset)
    {
        startOffset = new Vector2(data.Float("startOffsetX"), data.Float("startOffsetY"));
        placementOffset = new Vector2(data.Float("placementOffsetX"), data.Float("placementOffsetY"));
        pasteNumber = data.Int("pasteNumber");
        duplicateFlag = data.Attr("duplicateFlag", "LuckyHelper_DuplicateItemFlag");
        duplicateOnEnterRoom = data.Bool("duplicateOnEnterRoom");
        showDuplicatedOrderNumber = data.Bool("showDuplicatedOrderNumber");
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);

        if (duplicateOnEnterRoom)
        {
            DuplicateItems();
        }
    }

    public override void Update()
    {
        base.Update();
        if (this.TriggeredByFlag(duplicateFlag))
        {
            DuplicateItems();
        }
    }

    private void DuplicateItems()
    {
        foreach (PasteItem pasteItem in this.Tracker().GetEntities<PasteItem>())
        {
            bool pasteItemInTrigger = Left < pasteItem.CenterX && pasteItem.CenterX < Right && Top < pasteItem.CenterY && pasteItem.CenterY < Bottom;
            if (!pasteItemInTrigger)
                continue;
            DuplicateItem(pasteItem);
        }
    }

    private void DuplicateItem(PasteItem pasteItem)
    {
        for (int i = 0; i < pasteNumber; i++)
        {
            Vector2 extraOffset = startOffset + placementOffset * i;
            pasteItem.GenerateCopiedItems(extraOffset);

            if (showDuplicatedOrderNumber)
            {
                int order = i + 1;
                SimpleText simpleText = new SimpleText(order.ToString())
                {
                    Position = pasteItem.Center + extraOffset,
                    Scale = 0.5f
                };
                Scene.Add(simpleText);
            }
        }
    }
}