using LuckyHelper.Components;
using MonoMod.Utils;

namespace LuckyHelper.Handlers.Impl;

public class ZipMoverNodeHandler : EntityHandler, IMoveable, IAnchorProvider
{
	private ZipMover zipMover;
	private bool first;

	public ZipMoverNodeHandler(Entity entity, bool first) : base(entity)
	{
		zipMover = entity as ZipMover;
		this.first = first;

		if (first)
		{
			DynamicData.For(zipMover).Set("zipMoverNodeHandled", true);
		}
	}

	public override bool IsInside(EntityContainer container)
	{
		return InsideCheck(container, first, zipMover);
	}

	public override Rectangle GetBounds()
	{
		var pos = first ? zipMover.start : zipMover.target;
		return new Rectangle((int)pos.X, (int)pos.Y, 0, 0);
	}

	public bool Move(Vector2 move, Vector2? liftSpeed)
	{
		if (first)
		{
			zipMover.start += move;
		}
		else
		{
			zipMover.target += move;
		}

		var newPos = Vector2.Lerp(zipMover.start, zipMover.target, zipMover.percent);
		zipMover.MoveTo(newPos, zipMover.LiftSpeed);

		if (zipMover.pathRenderer != null)
		{
			UpdatePathRenderer(zipMover.start, zipMover.target);
		}

		return true;
	}

	private void UpdatePathRenderer(Vector2 newFrom, Vector2 newTo)
	{
		var pathRenderer = zipMover.pathRenderer;

		var from = newFrom + new Vector2(Entity.Width / 2f, Entity.Height / 2f);
		var to = newTo + new Vector2(Entity.Width / 2f, Entity.Height / 2f);
		var angle = (from - to).Angle();

		pathRenderer.from = from;
		pathRenderer.to = to;
		pathRenderer.sparkAdd = (from - to).SafeNormalize(5f).Perpendicular();
		pathRenderer.sparkDirFromA = angle + ((float)Math.PI / 8f);
		pathRenderer.sparkDirFromB = angle - ((float)Math.PI / 8f);
		pathRenderer.sparkDirToA = angle + (float)Math.PI - ((float)Math.PI / 8f);
		pathRenderer.sparkDirToB = angle + (float)Math.PI + ((float)Math.PI / 8f);
	}

	public void PreMove() { }

	// Don't use default anchor handling
	public List<string> GetAnchors() => new();


	public static bool InsideCheck(EntityContainer container, bool first, ZipMover entity)
	{
		var pos = first ? entity.start : entity.target;
		return pos.X >= container.Entity.Left && pos.Y >= container.Entity.Top &&
			pos.X <= container.Entity.Right && pos.Y <= container.Entity.Bottom;
	}
}
