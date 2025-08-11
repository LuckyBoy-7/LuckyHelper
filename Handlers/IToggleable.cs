namespace LuckyHelper.Handlers;

public interface IToggleable
{
	object GetDefaultState();

	object SaveState();

	void ReadState(object state, bool toggleActive, bool toggleVisible, bool toggleCollidable);

	void Disable(bool toggleActive, bool toggleVisible, bool toggleCollidable);
}
