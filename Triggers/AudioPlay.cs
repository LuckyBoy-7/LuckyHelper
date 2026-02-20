using Celeste.Mod.Entities;

namespace LuckyHelper.Triggers;

[CustomEntity("LuckyHelper/AudioPlay")]
[Tracked]
public class AudioPlay : Trigger
{
    private string audioEvent;
    private bool playOnEnterRoom;

    public AudioPlay(EntityData data, Vector2 offset) : base(data, offset)
    {
        audioEvent = data.Attr("audioEvent");
        playOnEnterRoom = data.Bool("playOnEnterRoom");
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        if (playOnEnterRoom)
            Play();
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        Play();
    }

    private void Play()
    {
        if (string.IsNullOrEmpty(audioEvent))
            return;
        Audio.Play(audioEvent, Center);
    }
}