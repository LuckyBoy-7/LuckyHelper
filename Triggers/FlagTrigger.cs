namespace LuckyHelper.Triggers;



public abstract class FlagTrigger : EntityTrigger
{
    protected string flag;
    protected FlagTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        flag = data.Attr("flag");
    }

    public override void OnTriggered()
    {
        TrySetFlag();
    }

    public abstract bool TrySetFlag();
}