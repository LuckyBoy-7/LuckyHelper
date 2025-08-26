using LuckyHelper.Components;

namespace LuckyHelper.Handlers;

public interface IModifyColor
{
	void BeforeRender(ColorModifierComponent modifier);
	void AfterRender(ColorModifierComponent modifier);
}
