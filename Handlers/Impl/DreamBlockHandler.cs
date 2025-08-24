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

public class DreamBlockHandler : EntityHandler
{
    public DreamBlockHandler(Entity entity) : base(entity)
    {
    }

    public override List<string> GetPossibleColorFields()
        => ["activeBackColor", "disabledBackColor", "activeLineColor", "disabledLineColor"];
}