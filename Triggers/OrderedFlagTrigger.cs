using Celeste.Mod.Entities;
using LuckyHelper.Extensions;
using LuckyHelper.Utils;

namespace LuckyHelper.Triggers;

public enum BeforeSetFlagAction
{
    None,
    ClearOtherFlags,
    ClearPreviousFlag,
}

public enum OnFlagReachEndAction
{
    Cycle,
    Over,
    PingPong,
    Random
}

[CustomEntity("LuckyHelper/OrderedFlagTrigger")]
[Tracked]
public class OrderedFlagTrigger : FlagTrigger
{
    public BeforeSetFlagAction BeforeSetFlagAction;
    public OnFlagReachEndAction OnFlagReachEndAction;
    public bool StartAtExistingFlag;
    public bool UpdateCursorByCurrentFlags;
    public int currentFlagIndex = -1;
    public int pingPongDir = 1;

    public OrderedFlagTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        BeforeSetFlagAction = data.Enum("beforeSetFlagAction", BeforeSetFlagAction.ClearOtherFlags);
        OnFlagReachEndAction = data.Enum("onFlagReachEndAction", OnFlagReachEndAction.Cycle);
        StartAtExistingFlag = data.Bool("startAtExistingFlag");
        UpdateCursorByCurrentFlags = data.Bool("updateCursorByCurrentFlags");
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        if (StartAtExistingFlag)
            for (int i = 0; i < Flags.Count; i++)
            {
                if (this.Session().GetFlag(Flags[i]))
                {
                    currentFlagIndex = i;
                    break;
                }
            }
    }

    public override void Update()
    {
        base.Update();
        if (UpdateCursorByCurrentFlags)
        {
            int targetFlagIndex = -1;
            for (int i = 0; i < Flags.Count; i++)
            {
                if (this.Session().GetFlag(Flags[i]))
                {
                    // 说明至少两个 flag 开着
                    if (targetFlagIndex != -1)
                        return;
                    targetFlagIndex = i;
                }
            }

            if (targetFlagIndex != -1)
            {
                currentFlagIndex = targetFlagIndex;
            }
        }
    }

    public override bool TrySetFlag()
    {
        Session session = this.Session();
        BeforeSetFlag();
        if (MoveCursor())
        {
            session.SetFlag(Flags[currentFlagIndex], true);
            return true;
        }

        return false;
    }

    private bool MoveCursor()
    {
        switch (OnFlagReachEndAction)
        {
            case OnFlagReachEndAction.Cycle:
                currentFlagIndex = (currentFlagIndex + 1) % Flags.Count;
                break;
            case OnFlagReachEndAction.Over:
                if (currentFlagIndex == Flags.Count - 1)
                {
                    RemoveSelf();
                    return false;
                }

                currentFlagIndex += 1;
                break;
            case OnFlagReachEndAction.PingPong:
                if (currentFlagIndex == Flags.Count - 1 && pingPongDir == 1 || currentFlagIndex == 0 && pingPongDir == -1)
                {
                    pingPongDir *= -1;
                }

                currentFlagIndex = (currentFlagIndex + pingPongDir + Flags.Count) % Flags.Count;
                break;
            case OnFlagReachEndAction.Random:
                if (currentFlagIndex == Flags.Count - 1)
                {
                    currentFlagIndex = Calc.Random.Range(0, Flags.Count);
                }
                else
                {
                    currentFlagIndex += 1;
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return true;
    }

    private void BeforeSetFlag()
    {
        switch (BeforeSetFlagAction)
        {
            case BeforeSetFlagAction.None:
                break;
            case BeforeSetFlagAction.ClearOtherFlags:
                Flags.ForEach(flag => this.Session().SetFlag(flag, false));
                break;
            case BeforeSetFlagAction.ClearPreviousFlag:
                if (currentFlagIndex != -1)
                    this.Session().SetFlag(Flags[currentFlagIndex], false);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}