using System.Reflection;
using LuckyHelper.Components;
using LuckyHelper.Components.EeveeLike;
using LuckyHelper.Extensions;
using LuckyHelper.Module;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace LuckyHelper.Handlers.Impl;

public class DreamBlockHandler : EntityHandler, IModifyColor
{
    private DreamBlock dreamBlock;

    public DreamBlockHandler(Entity entity) : base(entity)
    {
        dreamBlock = entity as DreamBlock;
    }

    public override List<string> GetPossibleColorFields()
        => ["activeBackColor", "disabledBackColor", "activeLineColor", "disabledLineColor"];


    public List<Color> Colors = new();

    public void BeforeRender(ColorModifierComponent modifier)
    {
        Colors.Clear();
        if (modifier.AffectTexture)
            for (int i = 0; i < dreamBlock.particles.Length; i++)
            {
                Colors.Add(dreamBlock.particles[i].Color);
                dreamBlock.particles[i].Color = modifier.GetHandledColor(dreamBlock.particles[i].Color);
            }
    }

    public void AfterRender(ColorModifierComponent modifier)
    {
        if (modifier.AffectTexture)
            for (int i = 0; i < dreamBlock.particles.Length; i++)
            {
                dreamBlock.particles[i].Color = Colors[i];
            }
    }
}