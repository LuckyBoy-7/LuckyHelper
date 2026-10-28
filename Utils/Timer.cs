namespace LuckyHelper.Utils;

public class Timer
{
    private float timer = -1;
    private float _duration;
    public float Duration => _duration;
    private Action callback;

    public Timer(float duration, Action callback = null)
    {
        this._duration = duration;
        this.callback = callback;
    }

    public void Consume()
    {
        timer = -1;
    }

    public void Restart()
    {
        timer = _duration;
    }

    public void Restart(float targetDuration)
    {
        timer = targetDuration;
    }

    /// <summary>
    /// 有的时候我们希望在定时器时间之内做某些事情
    /// </summary>
    /// <returns></returns>
    public bool InTime => timer > 0;

    public bool Over => !InTime;
    public float Value => timer;
    public float Percent => timer < 0 ? 1 : (_duration - timer) / _duration;


    /// <summary>
    /// 有的时候我们希望间隔一定时间触发一个事件
    /// </summary>
    /// <param name="deltaTime"></param>
    /// <returns></returns>
    public bool Update(float deltaTime)
    {
        if (_duration <= 0f || timer == -1)
            return false;

        timer -= deltaTime;
        if (timer <= 0)
        {
            callback?.Invoke();
            timer = -1;
            return true;
        }

        return false;
    }
}