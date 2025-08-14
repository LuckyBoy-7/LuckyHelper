using LuckyHelper.Components;
using LuckyHelper.Components.EeveeLike;

namespace LuckyHelper.Handlers.Impl;

class WaterHandler : EntityHandler, IMoveable
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

	public void PreMove() { }
}
