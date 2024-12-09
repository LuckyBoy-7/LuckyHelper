using Celeste.Mod.Entities;

namespace LuckyHelper.Triggers;

[CustomEntity("LuckyHelper/TimeSavedIn")]
[Tracked]
public class TimeSavedIn : Trigger
{
    public string SavedIn;
    
    public TimeSavedIn(EntityData data, Vector2 offset) : base(data, offset)
    {
        SavedIn = data.Attr("savedIn");
    }
}