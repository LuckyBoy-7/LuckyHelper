using LuckyHelper.Components;
using MonoMod.Utils;

namespace LuckyHelper.Handlers.Impl;

public class SwapBlockHandler : EntityHandler, IMoveable, IAnchorProvider
{
	public const string HandledString = "EeveeHelper_SwapBlockHandled";

	private SwapBlock swapBlock;
	private bool first;

	public SwapBlockHandler(Entity entity, bool first) : base(entity)
	{
		swapBlock = entity as SwapBlock;
		this.first = first;

		DynamicData.For(swapBlock).Set(HandledString, true);
	}

	public override bool IsInside(EntityContainer container)
	{
		return InsideCheck(container, first, swapBlock);
	}

	public override Rectangle GetBounds()
	{
		var pos = first ? swapBlock.start : swapBlock.end;
		return new Rectangle((int)pos.X, (int)pos.Y, 0, 0);
	}

	public bool Move(Vector2 move, Vector2? liftSpeed)
	{
		if (first)
		{
			swapBlock.start += move;
		}
		else
		{
			swapBlock.end += move;
		}

		var newPos = Vector2.Lerp(swapBlock.start, swapBlock.end, swapBlock.lerp);
		swapBlock.MoveTo(newPos, swapBlock.LiftSpeed);

		if (first)
		{
			swapBlock.moveRect.X = Math.Min((int)swapBlock.end.X, (int)swapBlock.start.X);
			swapBlock.moveRect.Y = Math.Min((int)swapBlock.end.Y, (int)swapBlock.start.Y);
		}

		return true;
	}

	public void PreMove() { }

	// Don't use default anchor handling
	public List<string> GetAnchors() => new();

	public static bool InsideCheck(EntityContainer container, bool first, SwapBlock entity)
	{
		var pos = first ? entity.start : entity.end;
		return pos.X >= container.Entity.Left && pos.Y >= container.Entity.Top &&
			pos.X + container.Entity.Width <= container.Entity.Right && pos.Y + container.Entity.Height <= container.Entity.Bottom;
	}
}
