using LuckyHelper.Components;
using LuckyHelper.Components.EeveeLike;

namespace LuckyHelper.Handlers.Impl;

internal class DecalHandler : EntityHandler, IMoveable
{
	public DecalHandler(Entity e) : base(e) { }

	public override bool IsInside(EntityContainer container)
	{
		return container.CheckDecal(Entity as Decal);
	}

	public bool Move(Vector2 move, Vector2? liftSpeed)
	{
		Entity.Position += move;
		return true;
	}

	public void PreMove() { }
}
