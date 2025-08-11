using LuckyHelper.Components;

namespace LuckyHelper.Handlers;

public interface IEntityHandler
{
	Entity Entity { get; }
	EntityContainer Container { get; set; }

	void OnAttach(EntityContainer container);

	void OnDetach(EntityContainer container);

	bool IsInside(EntityContainer container);

	Rectangle GetBounds();

	void Destroy();
}
