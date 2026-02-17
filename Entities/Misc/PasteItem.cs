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
    private string generateFlag;
    private bool pasteOnEnter;
    public bool pasteEntity;
    public bool pasteTrigger;
    public bool pasteForegroundDecal;
    public bool pasteBackgroundDecal;

    public Vector2 PositionRelativeToRoom;

    private Vector2 origPosition;


    public PasteItem(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        PositionRelativeToRoom = data.Position;
        origPosition = Position;
        PastedFromID = data.Attr("pastedFromID");
        generateFlag = data.Attr("generateFlag", "LuckyHelper_GenerateItemFlag");
        pasteOnEnter = data.Bool("pasteOnEnter", true);
        pasteEntity = data.Bool("pasteEntity");
        pasteTrigger = data.Bool("pasteTrigger");
        pasteForegroundDecal = data.Bool("pasteForegroundDecal");
        pasteBackgroundDecal = data.Bool("pasteBackgroundDecal");
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        if (pasteOnEnter)
            NewCopiedItems();
        
    }

    public override void Update()
    {
        base.Update();

        if (this.TriggeredByFlag(generateFlag))
        {
            NewCopiedItems();
        }
    }

    private void NewCopiedItems()
    {
        if (CopyItem.idToCopiedItemData.TryGetValue(PastedFromID, out var copiedItemData))
        {
            Level level = this.Level();

            // 因为 PasteItem 有可能会被移动, 所以还得算上后续场景中 PasteItem 的偏移
            Vector2 relativePosition = PositionRelativeToRoom + Position - origPosition;
            if (pasteEntity)
            {
                foreach (var entityDataWithOffset in copiedItemData.Entities)
                {
                    EntityData clonedEntityData = entityDataWithOffset.Data.Clone();
                    Vector2 offsetPosition = entityDataWithOffset.PivotToItemOffset + relativePosition;
                    clonedEntityData.Position = offsetPosition;

                    level.AddEntityWithEntityData(clonedEntityData);
                }
            }

            if (pasteTrigger)
            {
                foreach (var entityDataWithOffset in copiedItemData.Triggers)
                {
                    EntityData clonedEntityData = entityDataWithOffset.Data.Clone();
                    Vector2 offsetPosition = entityDataWithOffset.PivotToItemOffset + relativePosition;
                    clonedEntityData.Position = offsetPosition;

                    level.AddEntityWithTriggerData(clonedEntityData);
                }
            }

            if (pasteForegroundDecal)
            {
                foreach (var decalDataWithOffset in copiedItemData.FgDecals)
                {
                    DecalData clonedDecalData = decalDataWithOffset.Data.Clone();
                    Vector2 offsetPosition = decalDataWithOffset.PivotToItemOffset + relativePosition;
                    clonedDecalData.Position = offsetPosition;

                    level.AddEntityWithDecalData(clonedDecalData, true);
                }
            }

            if (pasteBackgroundDecal)
            {
                foreach (var decalDataWithOffset in copiedItemData.BgDecals)
                {
                    DecalData clonedDecalData = decalDataWithOffset.Data.Clone();
                    Vector2 offsetPosition = decalDataWithOffset.PivotToItemOffset + relativePosition;
                    clonedDecalData.Position = offsetPosition;

                    level.AddEntityWithDecalData(clonedDecalData, false);
                }
            }
        }
    }
}