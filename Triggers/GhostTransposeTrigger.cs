using Celeste.Mod.Entities;
using LuckyHelper.Modules;

namespace LuckyHelper.Triggers;

[CustomEntity("LuckyHelper/GhostTransposeTrigger")]
public class GhostTransposeTrigger : Trigger
{
    public bool EnableGhostTranspose;

    public enum GhostOutOfBoundsActions
    {
        None, // 什么都不做, 自动销毁
        TreatAsSolid, // 跟撞在solid上逻辑一样
        KillPlayer, // 杀死玩家
    }

    public GhostOutOfBoundsActions GhostOutOfBoundsAction;

    public enum TransposeDirTypes
    {
        TwoSides, // 左右
        EightSides, // 八方向
    }

    public TransposeDirTypes TransposeDirType;

    public int MaxGhostNumber;

    public float GhostSpeed;
    public bool UseDashKey; // 不使用dash key的话就用默认按键
    public Color Color;
    public float Alpha;
    public bool ReverseEnableOnExit;
    public bool KillPlayerOnTeleportToSpike;
    public bool ConserveMomentum;

    public GhostTransposeTrigger(EntityData data, Vector2 offset)
        : base(data, offset)
    {
        EnableGhostTranspose = data.Bool("enableGhostTranspose");
        GhostSpeed = data.Float("ghostSpeed");
        UseDashKey = data.Bool("useDashKey");
        Color = data.HexColor("color");
        Alpha = data.Float("alpha");
        GhostOutOfBoundsAction = data.Enum<GhostOutOfBoundsActions>("ghostOutOfBoundsAction");
        TransposeDirType = data.Enum<TransposeDirTypes>("transposeDirType");
        MaxGhostNumber = data.Int("maxGhostNumber");
        KillPlayerOnTeleportToSpike = data.Bool("killPlayerOnTeleportToSpike");
        ConserveMomentum = data.Bool("conserveMomentum");
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        GhostTransposeModule.EnableGhostTranspose = EnableGhostTranspose;
        GhostTransposeModule.GhostOutOfBoundsAction = GhostOutOfBoundsAction;
        GhostTransposeModule.GhostSpeed = GhostSpeed;
        GhostTransposeModule.UseDashKey = UseDashKey;
        GhostTransposeModule.Color = Color;
        GhostTransposeModule.Alpha = Alpha;
        GhostTransposeModule.MaxGhostNumber = MaxGhostNumber;
        GhostTransposeModule.TransposeDirType = TransposeDirType;
        GhostTransposeModule.KillPlayerOnTeleportToSpike = KillPlayerOnTeleportToSpike;
        GhostTransposeModule.ConserveMomentum = ConserveMomentum;
    }

    public override void OnLeave(Player player)
    {
        base.OnLeave(player);
        if (ReverseEnableOnExit)
            GhostTransposeModule.EnableGhostTranspose = !EnableGhostTranspose;
    }
}