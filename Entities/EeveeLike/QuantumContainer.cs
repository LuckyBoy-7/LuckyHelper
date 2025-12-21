using System.Numerics;
using Celeste.Mod.Entities;
using LuckyHelper.Components.EeveeLike;
using LuckyHelper.Extensions;
using LuckyHelper.Utils;
using YamlDotNet.Serialization.NodeDeserializers;
using CS06_Ending = On.Celeste.CS06_Ending;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;


namespace LuckyHelper.Entities.EeveeLike;

[Tracked]
[CustomEntity("LuckyHelper/QuantumContainer")]
public class QuantumContainer : Actor, IContainer
{
    public EntityContainer Container => _Container;
    public EntityContainerMover _Container;

    public Vector2[] QuantumPositions;
    private int currentIndex = 0;
    private bool preInsideCamera;

    public QuantumContainer(EntityData data, Vector2 offset) : base(data.Position + offset + new Vector2(data.Width / 2f, data.Height / 2f))
    {
        Collider = new Hitbox(data.Width, data.Height);
        Collider.Position = new Vector2(-Width / 2f, -Height / 2f);
        AllowPushing = false;

        Depth = -1000000;

        Add(_Container = new EntityContainerMover(data)
        {
            DefaultIgnored = e => e is QuantumContainer,
            OnFit = OnFit
        });

        QuantumPositions = data.NodesWithPosition(offset + this.HalfSize());
    }

    private void OnFit(Vector2 pos, float width, float height)
    {
        Position = new Vector2(pos.X + width / 2f, pos.Y + height / 2f);
        Collider.Position = new Vector2(-width / 2f, -height / 2f);
        Collider.Width = width;
        Collider.Height = height;
    }

    public override void Update()
    {
        base.Update();
        bool insideCamera = InsideCamera(currentIndex);
        // 离开观测状态的瞬间, 或者没被看到后隔一段时间更新(因为没被看到的位置对应的序列可能会变化)
        if (!insideCamera && (preInsideCamera || Scene.OnInterval(1f)))
            UpdateQuantumState();

        preInsideCamera = insideCamera;
    }

    private void UpdateQuantumState()
    {
        // 1/1, 1/2, 1/3 ... 1/n
        int nextIndex = currentIndex;
        int count = 1;
        for (int i = 0; i < QuantumPositions.Length; i++)
        {
            if (i != currentIndex && !InsideCamera(i))
            {
                if (Calc.Random.Next(count++) == 0)
                    nextIndex = i;
            }
        }

        if (nextIndex != currentIndex)
        {
            _Container.DoMoveAction(() =>
            {
                currentIndex = nextIndex;
                Position = QuantumPositions[currentIndex];
            });
        }
    }

    private bool InsideCamera(int index)
    {
        Vector2 origPosition = Position;
        Position = QuantumPositions[index];
        bool inside = InsideCamera(Collider.AbsoluteLeft, Collider.AbsoluteTop, Collider.AbsoluteRight, Collider.AbsoluteBottom);
        Position = origPosition;
        return inside;
    }

    private bool InsideCamera(float left, float top, float right, float bottom)
    {
        Level level = SceneAs<Level>();

        Vector2 topLeft = level.WorldToScreen(new Vector2(left, top)).Round();
        Vector2 bottomRight = level.WorldToScreen(new Vector2(right - 1, bottom - 1)).Round();

        // 好像开了拓展镜头之后会有那么一两个像素的误差, 先这样(
        return topLeft.X < 1920 && bottomRight.X >= 0 && bottomRight.Y >= 0 && topLeft.Y < 1080;
    }
}