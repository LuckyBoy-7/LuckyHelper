namespace LuckyHelper.Handlers.Impl;

public class RotateSpinnerHandler : EntityHandler, IAnchorProvider
{
	public RotateSpinnerHandler(Entity entity) : base(entity) { }

	public List<string> GetAnchors() => new() { "center" };
}
