namespace LuckyHelper.Module;

[AttributeUsage(AttributeTargets.Method)]
internal class UnloadAttribute : Attribute
{
    public UnloadAttribute()
    {
    }
}