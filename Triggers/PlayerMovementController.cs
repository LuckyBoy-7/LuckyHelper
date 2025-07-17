using Celeste.Mod.Entities;
using LuckyHelper.Module;

namespace LuckyHelper.Triggers;

public struct PlayerMovementData
{
    public float JumpForceX;
    public float JumpForceY;
    public float WallJumpForceX;
    public float WallJumpForceY;
    public float JumpKeepSpeedTime;
    public float WallJumpKeepSpeedTime;
}

[CustomEntity("LuckyHelper/PlayerMovementController")]
[Tracked]
public class PlayerMovementController : Trigger
{
    // public ActivationType ActivationType;
    public ActivationType ActivationType;
    public PlayerMovementData PlayerMovementData;

    public PlayerMovementController(EntityData data, Vector2 offset) : base(data, offset)
    {
        ActivationType = data.Enum<ActivationType>("activationType");
        PlayerMovementData = new PlayerMovementData()
        {
            JumpForceX = data.Float("jumpForceX"),
            JumpForceY = data.Float("jumpForceY"),
            WallJumpForceX = data.Float("wallJumpForceX"),
            WallJumpForceY = data.Float("wallJumpForceY"),
            JumpKeepSpeedTime = data.Float("jumpKeepSpeedTime"),
            WallJumpKeepSpeedTime = data.Float("wallJumpKeepSpeedTime"),
        };
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);

        var session = LuckyHelperModule.Session;
        if (ActivationType is ActivationType.Set or ActivationType.None)
        {
            // stay 状态 handler 会自己去找 trigger 里的 data 的, 所以 session 之用记录要不要用 set 参数即可
            session.playerUseSetMovementData = false;

            if (ActivationType == ActivationType.Set)
            {
                session.playerUseSetMovementData = true;
                session.PlayerMovementData = PlayerMovementData;
            }
        }
    }
}