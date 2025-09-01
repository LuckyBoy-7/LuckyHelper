using Celeste.Mod.Entities;
using FMOD.Studio;
using LuckyHelper.Module;
using LuckyHelper.Utils;
using Audio = On.Celeste.Audio;
using BloomRenderer = On.Celeste.BloomRenderer;
using LightingRenderer = On.Celeste.LightingRenderer;
using Player = On.Celeste.Player;

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
        On.Celeste.Audio.Play_string += AudioOnPlay_string;
        On.Celeste.Audio.Play_string_string_float += AudioOnPlay_string_string_float;
        On.Celeste.Audio.Play_string_Vector2 += AudioOnPlay_string_Vector2;
        On.Celeste.Audio.Play_string_Vector2_string_float += AudioOnPlay_string_Vector2_string_float;
        On.Celeste.Audio.Play_string_Vector2_string_float_string_float += AudioOnPlay_string_Vector2_string_float_string_float;
        On.Celeste.SoundSource.Play += SoundSourceOnPlay;
    }

    private static SoundSource SoundSourceOnPlay(On.Celeste.SoundSource.orig_Play orig, SoundSource self, string path, string param, float value)
    {
        var sound = orig(self, path, param, value);
        GetHandledEventInstance(path, sound.instance);
        return sound;
    }


    [Unload]
    public static void Unload()
    {
        On.Celeste.Audio.Play_string -= AudioOnPlay_string;
        On.Celeste.Audio.Play_string_string_float -= AudioOnPlay_string_string_float;
        On.Celeste.Audio.Play_string_Vector2 -= AudioOnPlay_string_Vector2;
        On.Celeste.Audio.Play_string_Vector2_string_float -= AudioOnPlay_string_Vector2_string_float;
        On.Celeste.Audio.Play_string_Vector2_string_float_string_float -= AudioOnPlay_string_Vector2_string_float_string_float;
        On.Celeste.SoundSource.Play -= SoundSourceOnPlay;
    }

    private static EventInstance AudioOnPlay_string_Vector2_string_float(Audio.orig_Play_string_Vector2_string_float orig, string path, Vector2 position, string param, float value)
    {
        return GetHandledEventInstance(path, orig(path, position, param, value));
    }

    private static EventInstance AudioOnPlay_string_Vector2(Audio.orig_Play_string_Vector2 orig, string path, Vector2 position)
    {
        return GetHandledEventInstance(path, orig(path, position));
    }

    private static EventInstance AudioOnPlay_string_string_float(Audio.orig_Play_string_string_float orig, string path, string param, float value)
    {
        return GetHandledEventInstance(path, orig(path, param, value));
    }

    private static EventInstance AudioOnPlay_string(Audio.orig_Play_string orig, string path)
    {
        return GetHandledEventInstance(path, orig(path));
    }

    private static EventInstance AudioOnPlay_string_Vector2_string_float_string_float(Audio.orig_Play_string_Vector2_string_float_string_float orig, string path, Vector2 position,
        string param, float value, string param2, float value2)
    {
        return GetHandledEventInstance(path, orig(path, position, param, value, param2, value2));
    }

    private static EventInstance GetHandledEventInstance(string path, EventInstance ins)
    {
        var dic = LuckyHelperModule.Session?.AudioNameToVolume;
        if (dic != null && dic.TryGetValue(path, out var volume))
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