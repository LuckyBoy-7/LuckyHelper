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
    private bool _affectPitch;
    private bool _replaceAudio;
    private List<string> _targets;
    private List<string> _replacedTargets;

    public const string AllToken = "all";

    public AudioAdjustTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        _targets = ParseUtils.ParseCommaSeperatedStringToList(data.Attr("targets"));
        _replacedTargets = ParseUtils.ParseCommaSeperatedStringToList(data.Attr("replacedTargets"));

        _affectVolume = data.Bool("affectVolume");
        _affectPitch = data.Bool("affectPitch");
        _replaceAudio = data.Bool("replaceAudio");
    }


    public override void OnSetValue(float factor)
    {
        var session = LuckyHelperModule.Session;
        for (var i = 0; i < _targets.Count; i++)
        {
            var target = _targets[i];
            if (_affectVolume)
                session.AudioNameToVolume[target] = factor;
            if (_affectPitch)
                session.AudioNameToPitch[target] = factor;
            if (_replaceAudio && i < _replacedTargets.Count)
                session.AudioNameToReplacedName[target] = _replacedTargets[i];
        }
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

    private static SoundSource SoundSourceOnPlay(On.Celeste.SoundSource.orig_Play orig, SoundSource self, string path, string param, float value)
    {
        var sound = orig(self, GetHandledPath(path), param, value);
        GetHandledEventInstance(path, sound.instance);
        return sound;
    }

    private static EventInstance AudioOnPlay_string_Vector2_string_float(Audio.orig_Play_string_Vector2_string_float orig, string path, Vector2 position, string param, float value)
    {
        return GetHandledEventInstance(path, orig(GetHandledPath(path), position, param, value));
    }

    private static EventInstance AudioOnPlay_string_Vector2(Audio.orig_Play_string_Vector2 orig, string path, Vector2 position)
    {
        return GetHandledEventInstance(path, orig(GetHandledPath(path), position));
    }

    private static EventInstance AudioOnPlay_string_string_float(Audio.orig_Play_string_string_float orig, string path, string param, float value)
    {
        return GetHandledEventInstance(path, orig(GetHandledPath(path), param, value));
    }

    private static EventInstance AudioOnPlay_string(Audio.orig_Play_string orig, string path)
    {
        return GetHandledEventInstance(path, orig(GetHandledPath(path)));
    }

    private static EventInstance AudioOnPlay_string_Vector2_string_float_string_float(Audio.orig_Play_string_Vector2_string_float_string_float orig, string path, Vector2 position,
        string param, float value, string param2, float value2)
    {
        return GetHandledEventInstance(path, orig(GetHandledPath(path), position, param, value, param2, value2));
    }

    private static string GetHandledPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return path;
        var dic = LuckyHelperModule.Session?.AudioNameToReplacedName;
        if (dic != null)
        {
            if (dic.TryGetValue(path, out var replacedPath1))
                return replacedPath1;
            if (dic.TryGetValue(AllToken, out var replacedPath2))
                return replacedPath2;
        }

        return path;
    }

    private static EventInstance GetHandledEventInstance(string path, EventInstance ins)
    {
        if (string.IsNullOrEmpty(path))
            return ins;
        var audioNameToVolume = LuckyHelperModule.Session?.AudioNameToVolume;
        if (audioNameToVolume != null)
        {
            if (audioNameToVolume.TryGetValue(path, out var volume1))
                ins.setVolume(volume1);
            if (audioNameToVolume.TryGetValue(AllToken, out var volume2))
                ins.setVolume(volume2);
        }

        var audioNameToPitch = LuckyHelperModule.Session?.AudioNameToPitch;
        if (audioNameToPitch != null)
        {
            if (audioNameToPitch.TryGetValue(path, out var pitch1))
                ins.setPitch(pitch1);
            if (audioNameToPitch.TryGetValue(AllToken, out var pitch2))
                ins.setPitch(pitch2);
        }

        return ins;
    }
}