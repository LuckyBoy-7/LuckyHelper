using Celeste.Mod.Entities;
using LuckyHelper.Modules;
using LuckyHelper.Module;

namespace LuckyHelper.Triggers;

[CustomEntity("LuckyHelper/SetFallingBlockBlockFloatySpaceBlockTrigger")]
class SetFallingBlockBlockFloatySpaceBlockTrigger : Trigger
{
    private bool enable;

    public SetFallingBlockBlockFloatySpaceBlockTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        Position = data.Position + offset;
        enable = data.Bool("enable");
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        LuckyHelperModule.Session.EnableFallingBlockBlocksFloatySpaceBlock = enable;
    }
}