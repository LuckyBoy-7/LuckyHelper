using System.Collections;
using Celeste.Mod.Entities;
using LuckyHelper.Components.EeveeLike;
using LuckyHelper.Extensions;

namespace LuckyHelper.Triggers;

[CustomEntity("LuckyHelper/DetachFollowerContainerTrigger")]
[Tracked]
public class DetachFollowerContainerTrigger : EntityTrigger
{
    protected Vector2 _targetPosition;

    public DetachFollowerContainerTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        Vector2[] nodes = data.NodesOffset(offset);
        _targetPosition = Center;
        if (nodes.Length != 0)
        {
            _targetPosition = nodes[0];
        }
    }

    public override void OnTriggered()
    {
        var player = this.Tracker().GetEntity<Player>();
        for (int i = player.Leader.Followers.Count - 1; i >= 0; i--)
        {
            if (player.Leader.Followers[i].Entity is FollowerContainer)
            {
                Add(new Coroutine(DetatchFollower(player.Leader.Followers[i])));
            }
        }
    }

    private IEnumerator DetatchFollower(Follower follower)
    {
        // 防止多次 attach
        if (!follower.HasLeader)
        {
            yield break;
        }

        Leader leader = follower.Leader;
        FollowerContainer entity = follower.Entity as FollowerContainer;
        // 防止 container 过去的时候又被 player 吸住了
        entity.Collidable = false;
        float distance = (entity.Position - _targetPosition).Length();
        float time = distance / 200f;

        leader.LoseFollower(follower);

        Audio.Play("event:/game/general/strawberry_touch", entity.Position);
        Vector2 startPosition = entity.Position;
        SimpleCurve curve = new SimpleCurve(startPosition, _targetPosition, startPosition + (_targetPosition - startPosition) * 0.5f + new Vector2(0f, -64f));
        for (float p = 0f; p < 1f; p += Engine.DeltaTime / time)
        {
            entity._Container.DoMoveAction(() => entity.Position = curve.GetPoint(Ease.CubeInOut(p)));
            yield return null;
        }

        entity._Container.DoMoveAction(() => entity.Position = _targetPosition);
        entity.Collidable = true;
    }
}