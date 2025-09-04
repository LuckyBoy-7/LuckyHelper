using Lucky.Kits.Collections;
using LuckyHelper.Handlers;
using LuckyHelper.Handlers.Impl;
using LuckyHelper.Modules;
using LuckyHelper.Utils;
using Microsoft.Xna.Framework.Graphics;

namespace LuckyHelper.Module;

public class LuckyHelperEffects
{
    public static DefaultDict<string, Effect> LuckyHelperEffect = new(() => null);
    public static Effect CustomBloomBlendEffect => LuckyHelperEffect[CustomBloomBlend];
    public const string CustomBloomBlend = "CustomBloomBlend";


    public static void LoadContent()
    {
        TryLoadEffect(CustomBloomBlend);
    }


    // https://github.com/HiBlueBerry/DBBHelper/blob/e55eb66e8a9f872b193cd2a9c8e92afda6441104/Source/DBB_Mechanism/SourceManager/DBBEffectSourceManager.cs#L57
    // 读取特效.cso文件
    private static void TryLoadEffect(string path)
    {
        // 如果特效已经存在
        if (string.IsNullOrEmpty(path))
        {
            LogUtils.LogWarning("Failed to load LuckyHelperEffect,because path in null.");
            return;
        }

        if (LuckyHelperEffect.ContainsKey(path))
        {
            LogUtils.LogInfo("LuckyHelperEffect: " + path + " has existed.");
            return;
        }

        // 如果特效不存在, 则尝试读取特效
        if (Everest.Content.TryGet("Effects/LuckyHelper/" + path + ".cso", out var effect_asset))
        {
            LuckyHelperEffect.Add(path, new Effect(Engine.Graphics.GraphicsDevice, effect_asset.Data));
        }
        else
        {
            LogUtils.LogWarning("Failed to load LuckyHelperEffect: " + path);
        }
    }
}