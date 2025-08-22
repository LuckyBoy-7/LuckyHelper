using Celeste.Mod.Entities;
using LuckyHelper.Module;
using LuckyHelper.Utils;
using BloomRenderer = On.Celeste.BloomRenderer;
using LightingRenderer = On.Celeste.LightingRenderer;

namespace LuckyHelper.Triggers;

public abstract class PositionTrigger : Trigger
{
    private float offsetFrom = 0;
    private float offsetTo = 2;


    private PositionModes positionMode;

    public PositionTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        offsetFrom = data.Float("offsetFrom");
        offsetTo = data.Float("offsetTo");
        positionMode = data.Enum<PositionModes>("positionMode");
    }

    public abstract void OnSetValue(float factor);


    public override void OnStay(Player player)
    {
        base.OnStay(player);
        float factor = MathHelper.Lerp(offsetFrom, offsetTo, GetPositionLerp(player, positionMode));
        OnSetValue(factor);
    }
}