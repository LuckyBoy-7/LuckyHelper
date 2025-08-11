namespace LuckyHelper.Extensions;

public static class TriggerExtensions
{
    public static void CoverRoom(this Trigger trigger)
    {
        Rectangle roomBounds = trigger.Level().Bounds;
        trigger.Position = roomBounds.Position();
        trigger.Collider.Width = roomBounds.Width;
        trigger.Collider.Height = roomBounds.Height;
    }
}