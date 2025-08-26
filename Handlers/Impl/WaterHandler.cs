using LuckyHelper.Components;
using LuckyHelper.Components.EeveeLike;

namespace LuckyHelper.Handlers.Impl;

class WaterHandler : EntityHandler, IMoveable, IModifyColor
{
    private Water water;

    public WaterHandler(Entity entity) : base(entity)
    {
        water = entity as Water;
    }

    public bool Move(Vector2 move, Vector2? liftSpeed)
    {
        if (!(Container as EntityContainerMover).IgnoreAnchors)
        {
            foreach (var surface in water.Surfaces)
            {
                surface.Position += move;
            }
        }

        return false;
    }

    public void PreMove()
    {
    }


    public List<List<Color>> SurfaceColors = new();

    public void BeforeRender(ColorModifierComponent modifier)
    {
        SurfaceColors.Clear();
        for (var i = 0; i < water.Surfaces.Count; i++)
        {
            Water.Surface surface = water.Surfaces[i];
            List<Color> colors = new();
            for (var index = 0; index < surface.mesh.Length; index++)
            {
                var vertexPositionColor = surface.mesh[index];
                colors.Add(vertexPositionColor.Color);
                surface.mesh[index].Color = modifier.GetHandledColor(surface.mesh[index].Color);
            }

            SurfaceColors.Add(colors);
        }
    }

    public void AfterRender(ColorModifierComponent modifier)
    {
        for (var i = 0; i < water.Surfaces.Count; i++)
        {
            Water.Surface surface = water.Surfaces[i];
            for (var index = 0; index < surface.mesh.Length; index++)
            {
                surface.mesh[index].Color = SurfaceColors[i][index];
            }
        }
    }
}