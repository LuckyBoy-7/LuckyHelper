namespace LuckyHelper.Utils;

public class DelegateValue<T>
{
    public T Value // cameraoffset的某一个维度
    {
        get => getter();
        set => setter(value);
    }

    private readonly Func<T> getter;
    private readonly Action<T> setter;

    public DelegateValue(Func<T> getter, Action<T> setter)
    {
        this.getter = getter;
        this.setter = setter;
    }
}