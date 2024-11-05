using System.Reflection;

namespace LuckyHelper.Utils;

internal static class AttributeUtils
{
    private static readonly object[] Parameterless = [];

    private static readonly IDictionary<Type, IEnumerable<MethodInfo>> MethodInfos = new Dictionary<Type, IEnumerable<MethodInfo>>();

    /// <summary>
    /// 注册类里的对应方法们
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static void CollectMethods<T>() where T : Attribute
    {
        // select是把一个对象cast到另一个对象
        // selectMany是把一个对象cast到多个对象（然后解包yield return）
        MethodInfos[typeof(T)] = Enumerable.SelectMany<Type, MethodInfo>(
            typeof(AttributeUtils).Assembly.GetTypesSafe(), // 拿到命名空间下的各种type
            // 注册每个类上带自定义属性的无参方法
            type => type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Where(
                info => info.GetParameters().Length == 0 && info.GetCustomAttribute<T>() != null
            )
        );
    }

    /// <summary>
    /// 执行类里的对应方法们
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static void Invoke<T>() where T : Attribute
    {
        if (MethodInfos.TryGetValue(typeof(T), out IEnumerable<MethodInfo> enumerable))
        {
            foreach (MethodInfo methodInfo in enumerable)
            {
                methodInfo.Invoke(null, Parameterless);
            }
        }
    }
}