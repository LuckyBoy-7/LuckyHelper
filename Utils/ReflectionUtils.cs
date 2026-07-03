using System.Reflection;

namespace LuckyHelper.Utils;

public class ReflectionUtils
{
    // 判断 type 是否重载了名为 methodName 的虚方法(不看基类, 只看这个具体类型自己是否声明了它)
    public static bool OverridesMethod(Type type, string methodName, Type[] param)
    {
        var method = type.GetMethod(
            methodName,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            param
        );

        if (method == null)
            return false; // 压根没有这个方法

        // DeclaringType 是"实际声明这个方法实现的类型"
        // 如果 DeclaringType 就是 type 本身, 说明 type 重写了它
        // 如果 DeclaringType 是某个基类, 说明 type 没有重写, 用的是继承来的实现
        return method.DeclaringType == type;
    }
}