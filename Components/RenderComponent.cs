namespace LuckyHelper.Components;

public class RenderComponent(bool active, bool visible) : Component(active, visible)
{
    public Action OnRender;

    public override void Render()
    {
        base.Render();
        OnRender?.Invoke();
    }
}