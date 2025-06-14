using Celeste.Mod.Entities;
using LuckyHelper.Extensions;

namespace LuckyHelper.Triggers;

[CustomEntity("LuckyHelper/TalkComponentController")]
[Tracked]
public class TalkComponentController : Trigger
{
    private string enableIfFlag;
    private TalkComponent talkComponent;

    public TalkComponentController(EntityData data, Vector2 offset) : base(data, offset)
    {
        enableIfFlag = data.Attr("enableIfFlag");
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        foreach (Entity entity in scene.Entities)
        {
            TalkComponent t = (TalkComponent)entity.Components.components.Find(component => component is TalkComponent);
            if (t != null)
            {
                Rectangle bounds = new Rectangle((int)(t.Entity.X + t.Bounds.X), (int)(t.Entity.Y + t.Bounds.Y), t.Bounds.Width, t.Bounds.Height);
                if (CollideRect(bounds))
                {
                    talkComponent = t;
                    break; // 找到第一个就行了
                }
            }
        }
    }

    public override void Update()
    {
        base.Update();
        if (talkComponent == null)
            return;
        bool on = this.Session().GetFlag(enableIfFlag);
        talkComponent.Enabled = on;
    }
}