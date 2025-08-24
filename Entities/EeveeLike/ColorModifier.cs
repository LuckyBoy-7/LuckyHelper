using Celeste.Mod.Entities;
using System.Collections;
using System.Reflection;
using LuckyHelper.Components;
using LuckyHelper.Components.EeveeLike;
using LuckyHelper.Extensions;
using LuckyHelper.Module;
using LuckyHelper.Modules;
using LuckyHelper.Utils;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using DashBlock = On.Celeste.DashBlock;


namespace LuckyHelper.Entities.EeveeLike;

[Tracked]
[CustomEntity("LuckyHelper/ColorModifier")]
public class ColorModifier : Actor, IContainer
{
    public EntityContainer Container { get; }
    // public List<Color> Colors;


    public ColorModifier(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        Collider = new Hitbox(data.Width, data.Height);

        Depth = Depths.Top - 10;

        // Colors = data.ParseColorList("colors");
 

        Add(Container = new EntityContainer(data)
        {
            DefaultIgnored = e => e is ColorModifier,
            OnAttach = handler => { handler.Entity.AddNoDuplicatedComponent(new ColorModifierComponent()
            {
                Colors = data.ParseColorList("colors"),
                AffectTexture = data.Bool("affectTexture", true),
                AffectLight = data.Bool("affectLight", true),
                AffectGeometry = data.Bool("affectGeometry", true),
            }); },
            OnDetach = handler => { handler.Entity.Get<ColorModifierComponent>()?.RemoveSelf(); }
        });
    }
}