using Celeste.Mod.Entities;

namespace LuckyHelper.Triggers;

[CustomEntity("LuckyHelper/DeathCountSavedIn")]
[Tracked]
public class DeathCountSavedIn : Trigger
{
    public string SavedIn;
    
    public DeathCountSavedIn(EntityData data, Vector2 offset) : base(data, offset)
    {
        SavedIn = data.Attr("savedIn");
    }
}