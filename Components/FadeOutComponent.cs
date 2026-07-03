namespace LuckyHelper.Components;

public class FadeOutComponent : Component
{
    private float timer;
    private float duration;
    private Action<float> OnFade;
    private Action OnFadeEnd;


    public FadeOutComponent(float duration, Action<float> OnFade, Action OnFadeEnd) : base(true, true)
    {
        timer = this.duration = duration;
        this.OnFade = OnFade;
        this.OnFadeEnd = OnFadeEnd;
    }

    public override void Update()
    {
        base.Update();

        timer -= Engine.DeltaTime;
        OnFade?.Invoke(MathHelper.Clamp(timer / duration, 0f, 1f));

        if (timer <= 0)
        {
            OnFadeEnd?.Invoke();
        }
    }
}