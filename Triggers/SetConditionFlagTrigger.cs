using Celeste.Mod.Entities;
using LuckyHelper.Extensions;
using LuckyHelper.Module;

namespace LuckyHelper.Triggers;

public enum SetConditionFlagTriggerStateType
{
    OnJump
}

public struct SetConditionFlagTriggerData
{
    public bool On;
    public SetConditionFlagTriggerStateType stateType;
    public string Flag;
    public int RemoveFlagDelayedFrames;
}

[CustomEntity("LuckyHelper/SetConditionFlagTrigger")]
[Tracked]
public class SetConditionFlagTrigger : Trigger
{
    public SetConditionFlagTriggerData flagData;

    public SetConditionFlagTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        flagData = new SetConditionFlagTriggerData()
        {
            On = data.Bool("on"),
            stateType = data.Enum<SetConditionFlagTriggerStateType>("stateType"),
            Flag = data.Attr("flag"),
            RemoveFlagDelayedFrames = data.Int("removeFlagDelayedFrames"),
        };
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        LuckyHelperSession session = LuckyHelperModule.Session;
        Dictionary<SetConditionFlagTriggerStateType, SetConditionFlagTriggerData> stateToDatas = session.SetConditionFlagTriggerStateToDatas;
        stateToDatas[flagData.stateType] = flagData;
    }
}