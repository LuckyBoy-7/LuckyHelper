using Celeste.Mod.Entities;
using LuckyHelper.Module;

namespace LuckyHelper.Triggers;

[CustomEntity("LuckyHelper/SetPassByRefillDashesTrigger")]
public class SetPassByRefillDashesTrigger : Trigger
{
    public int Dashes;

    public SetPassByRefillDashesTrigger(EntityData data, Vector2 offset)
        : base(data, offset)
    {
        Dashes = data.Int("dashes");
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        LuckyHelperModule.Session.RoomIdToPassByRefillDahes[SceneAs<Level>().Session.LevelData.Name] = Dashes;
    }


}