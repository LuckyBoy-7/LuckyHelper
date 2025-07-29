using Celeste.Mod.Entities;
using LuckyHelper.Extensions;
using LuckyHelper.Module;
using LuckyHelper.Utils;

namespace LuckyHelper.Triggers;

public enum FlagTriggerMode
{
    OnEntityEnter,
    OnEntityStay,
    OnEntityLeave,
    Always
}

public abstract class FlagTrigger : EntityTrigger
{
    public FlagTriggerMode FlagTriggerMode;

    public FlagTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        FlagTriggerMode = data.Enum<FlagTriggerMode>("flagTriggerMode");
    }

    public override void Update()
    {
        base.Update();
        if (FlagTriggerMode == FlagTriggerMode.Always)
            TrySetFlag();
    }

    public override void OnCustomEnter(Entity entity)
    {
        if (FlagTriggerMode == FlagTriggerMode.OnEntityEnter)
            TrySetFlag();
            
    }

    public override void OnCustomLeave(Entity entity)
    {
        if (FlagTriggerMode == FlagTriggerMode.OnEntityLeave)
            TrySetFlag();
    }

    public override void OnCustomStay(Entity entity)
    {
        if (FlagTriggerMode == FlagTriggerMode.OnEntityStay)
            TrySetFlag();
    }

    public abstract bool TrySetFlag();
}