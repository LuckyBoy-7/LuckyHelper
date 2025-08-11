namespace LuckyHelper.Handlers.Impl;

public class TrackSpinnerHandler : EntityHandler, IAnchorProvider
{
	public TrackSpinnerHandler(Entity entity) : base(entity) { }

	public List<string> GetAnchors() => new() { "Start", "End" };
}
