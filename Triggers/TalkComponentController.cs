using Celeste.Mod.Entities;
using LuckyHelper.Extensions;
using LuckyHelper.Modules;
using LuckyHelper.Utils;
using Microsoft.Xna.Framework.Input;

namespace LuckyHelper.Triggers;

[CustomEntity("LuckyHelper/TalkComponentController")]
[Tracked]
public class TalkComponentController : Trigger
{
    // private string enableIfFlag;
    private string hideIfFlag;
    private List<TalkComponent> collidedTalkComponents = new();
    private bool dynamic;
    private HashSet<string> blackLists;

    public TalkComponentController(EntityData data, Vector2 offset) : base(data, offset)
    {
        // enableIfFlag = data.Attr("enableIfFlag");
        hideIfFlag = data.Attr("hideIfFlag");
        dynamic = data.Bool("dynamic"); // 是不断判断呢, 还是只在一开始记录呢
        blackLists = data.Attr("blackList").Split(",").Select(s => s.Trim()).ToHashSet();
        Depth = 10;
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);

        if (!dynamic)
            FillAllCollidedTalkComponents(collidedTalkComponents);
    }

    private void FillAllCollidedTalkComponents(List<TalkComponent> lst)
    {
        foreach (TalkComponent talkComponent in TypeToObjectsModule.BriefTypeToComponents["TalkComponent"])
        {
            if (CollideWithTalkComponent(talkComponent) && !blackLists.Contains(talkComponent.Entity.BriefTypeName()))
            {
                lst.Add(talkComponent);
            }
        }
    }

    private bool CollideWithTalkComponent(TalkComponent t)
    {
        Rectangle bounds = new Rectangle((int)(t.Entity.X + t.Bounds.X), (int)(t.Entity.Y + t.Bounds.Y), t.Bounds.Width, t.Bounds.Height);
        return CollideRect(bounds);
    }

    public override void Update()
    {
        base.Update();
        if (dynamic)
        {
            collidedTalkComponents.Clear();
            FillAllCollidedTalkComponents(collidedTalkComponents);
        }

        if (collidedTalkComponents.Count == 0)
            return;

        bool on = !this.Session().GetFlag(hideIfFlag);
        foreach (var collidedTalkComponent in collidedTalkComponents)
        {
            collidedTalkComponent.Enabled = on;
            if (collidedTalkComponent.UI != null)
            {
                collidedTalkComponent.UI.Depth = Depth - 1;
            }
        }
    }
}