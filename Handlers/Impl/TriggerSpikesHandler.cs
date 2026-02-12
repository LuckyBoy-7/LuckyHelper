using LuckyHelper.Components;
using LuckyHelper.Components.EeveeLike;

namespace LuckyHelper.Handlers.Impl;

class TriggerSpikesHandler : EntityHandler, IModifyColor
{
    private TriggerSpikes triggerSpikes;

    public TriggerSpikesHandler(Entity entity) : base(entity)
    {
        triggerSpikes = entity as TriggerSpikes;
    }


    private Color[] tentacleColors;

    public void BeforeRender(ColorModifierComponent modifier)
    {
        tentacleColors = triggerSpikes.tentacleColors.ToArray();
        if (modifier.AffectGeometry)
            for (var i = 0; i < triggerSpikes.tentacleColors.Length; i++)
            {
                triggerSpikes.tentacleColors[i] = modifier.GetHandledColor(triggerSpikes.tentacleColors[i]);
            }
    }

    public void AfterRender(ColorModifierComponent modifier)
    {
        if (modifier.AffectGeometry)
            for (var i = 0; i < triggerSpikes.tentacleColors.Length; i++)
            {
                triggerSpikes.tentacleColors[i] = tentacleColors[i];
            }
    }
}