using System.Reflection;

namespace LuckyHelper.Utils;

public static class FieldUtils
{
    /// 如果拿父类的私有方法那么要写<父类类型>(子类类型， 字段名)
    public static object GetField<T>(T instance, string fieldName)
    {
        return typeof(T).GetField(fieldName,  BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(instance);
    }
}