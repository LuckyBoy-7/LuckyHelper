using Celeste.Mod.Entities;
using LuckyHelper.Extensions;
using LuckyHelper.Module;

namespace LuckyHelper.Triggers;

public enum SetConditionFlagTriggerStateType
{
    OnJump
}

public enum SetFlagConditionType
{
    OnJump
}

public struct SetConditionFlagTriggerData
{
    public SetFlagConditionType ConditionType;
    public ActivationType ActivationType;
    public string Flag;
    public int RemoveFlagDelayedFrames;
}

[CustomEntity("LuckyHelper/SetConditionFlagTrigger")]
[Tracked]
public class SetConditionFlagTrigger : Trigger
{
    public SetConditionFlagTriggerData flagData;
    private bool coverRoom;

    public SetConditionFlagTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        flagData = new SetConditionFlagTriggerData()
        {
            ConditionType = data.Enum<SetFlagConditionType>("conditionType"),
            ActivationType = data.Enum<ActivationType>("activationType"),
            Flag = data.Attr("flag"),
            RemoveFlagDelayedFrames = data.Int("removeFlagDelayedFrames"),
        };
        if (data.Has("on"))
        {
            bool on = data.Bool("on");
            flagData.ActivationType = on ? ActivationType.Set : ActivationType.None;
        }

        if (data.Has("stateType"))
        {
            SetConditionFlagTriggerStateType stateType = data.Enum<SetConditionFlagTriggerStateType>("stateType");
            if (stateType == SetConditionFlagTriggerStateType.OnJump)
                flagData.ConditionType = SetFlagConditionType.OnJump;
        }

        coverRoom = data.Bool("coverRoom");
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        if (coverRoom)
        {
            this.CoverRoom();
        }
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        LuckyHelperSession session = LuckyHelperModule.Session;
        Dictionary<SetFlagConditionType, SetConditionFlagTriggerData> stateToDatas = session.SetConditionFlagTriggerStateToDatas;
        if (flagData.ActivationType is ActivationType.None or ActivationType.Set)
            stateToDatas[flagData.ConditionType] = flagData;
    }
}