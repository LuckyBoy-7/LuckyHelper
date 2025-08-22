using Celeste.Mod.Entities;
using FMOD.Studio;
using LuckyHelper.Module;
using LuckyHelper.Utils;
using Audio = On.Celeste.Audio;
using BloomRenderer = On.Celeste.BloomRenderer;
using LightingRenderer = On.Celeste.LightingRenderer;

namespace LuckyHelper.Triggers;

[CustomEntity("LuckyHelper/AudioAdjustTrigger")]
[Tracked]
public class AudioAdjustTrigger : PositionTrigger
{
    private bool _affectVolume;
    private List<string> _targets = new();

    public AudioAdjustTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        _targets = ParseUtils.ParseCommaSeperatedStringToList(data.Attr("targets"));

        _affectVolume = data.Bool("affectVolume");
    }

    [Load]
    public static void Load()
    {
        On.Celeste.Audio.Play_string_Vector2_string_float_string_float += AudioOnPlay_string_Vector2_string_float_string_float;
    }


    [Unload]
    public static void Unload()
    {
        On.Celeste.Audio.Play_string_Vector2_string_float_string_float -= AudioOnPlay_string_Vector2_string_float_string_float;
    }

    private static EventInstance AudioOnPlay_string_Vector2_string_float_string_float(Audio.orig_Play_string_Vector2_string_float_string_float orig, string path, Vector2 position,
        string param, float value, string param2, float value2)
    {
        EventInstance ins = orig(path, position, param, value, param2, value2);
        var dic = LuckyHelperModule.Session.AudioNameToVolume;
        if (dic.TryGetValue(path, out var volume))
        {
            ins.setVolume(volume);
        }

        return ins;
    }


    public override void OnSetValue(float factor)
    {
        var session = LuckyHelperModule.Session;
        foreach (string target in _targets)
        {
            if (_affectVolume)
                session.AudioNameToVolume[target] = factor;
        }
    }
}