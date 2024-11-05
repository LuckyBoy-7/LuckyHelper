namespace LuckyHelper.Module;

[AttributeUsage(AttributeTargets.Method)]
internal class InitializeAttribute : Attribute
{
    public InitializeAttribute()
    {
    }
}