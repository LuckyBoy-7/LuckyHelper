namespace LuckyHelper.Extensions;

internal static class EntityDataExtensions
{
    /// <summary>
    /// 适配一些实体的老的Key
    /// </summary>
    /// <param name="data"></param>
    /// <param name="callback">key, 返回值, 默认值</param>
    /// <param name="keys">第一个为最新 key, 后续为老的 key</param>
    /// <typeparam name="T">要适配的数据类型</typeparam>
    /// <returns></returns>
    public static T Fit<T>(EntityData data, Func<string, T, T> callback, T defaultValue, params string[] keys)
    {
        for (int i = 0; i < keys.Length; i++)
        {
            if (data.Has(keys[i]))
            {
                return callback(keys[i], defaultValue);
            }
        }

        return defaultValue;
    }

    public static float FitFloat(this EntityData data, float defaultValue, params string[] keys) => Fit(data, data.Float, defaultValue, keys);
    public static Color FitColor(this EntityData data, Color defaultValue, params string[] keys) => Fit(data, data.HexColor, defaultValue, keys);
}