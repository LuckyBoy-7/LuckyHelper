#if DEBUG
using Celeste.Mod.Entities;

namespace LuckyHelper.Entities;


public class TestEntity : Solid
{
    public TestEntity(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, false)
    {

    }
}
#endif