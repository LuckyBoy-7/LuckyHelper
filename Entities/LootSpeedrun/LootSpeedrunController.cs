using System.Collections;
using Celeste.Mod.Entities;
using LuckyHelper.Extensions;
using Microsoft.Xna.Framework.Input;

namespace LuckyHelper.Entities.LootSpeedrun;

[CustomEntity("LuckyHelper/LootSpeedrunController")]
[Tracked]
public class LootSpeedrunController : Entity
{
    public const string LootMaxCreditsID = "LootMaxCredits";
    private LootSpeedrunInfoDisplay display;

    private float leftTime = 0;
    private string ID => $"{this.Session().LevelData.Name}_LootSpeedrunController";
    private string LeftTimeID => ID + "_LeftTime";
    private string CurrentValueID => ID + "_CurrentValue";
    private int curValue;

    // -----------------------------------------------------------------------
    private float totalTime = 100;
    private float timeReduceSpeedMultiplier = 5;
    private string teleportToRoomNameWhenTimeOver = "";
    private bool exitWhenLootsAllCollected = true;

    public LootSpeedrunController(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        // 保证最后更新最后, 删除restart flag
        Depth = -100000000;
        totalTime = data.Float("totalTime");
        timeReduceSpeedMultiplier = data.Float("timeReduceSpeedMultiplier");
        teleportToRoomNameWhenTimeOver = data.Attr("teleportToRoomNameWhenTimeOver");
        exitWhenLootsAllCollected = data.Bool("exitWhenLootsAllCollected");

        leftTime = totalTime;
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        if (Scene.Tracker.GetEntity<LootSpeedrunInfoDisplay>() == null)
            Scene.Add(display = new LootSpeedrunInfoDisplay());
        display ??= Scene.Tracker.GetEntity<LootSpeedrunInfoDisplay>();

        // 新的一局
        if (this.Session().GetFlag(LootSpeedrunStartup.LootRestartID))
        {
            foreach (Loot loot in this.Tracker().GetEntities<Loot>())
            {
                // 恢复收集状态
                this.Session().SetFlag(loot.CollectedID, false);
                // 恢复时间状态
                this.Session().SetCounter(LeftTimeID, (int)(totalTime * 1000));
                // todo: 恢复积分状态
                this.Session().SetCounter(CurrentValueID, 0);
                // 删除restart状态
                this.Session().SetFlag(LootSpeedrunStartup.LootRestartID, false);
            }
        }
        else
        {
            foreach (Loot loot in this.Tracker().GetEntities<Loot>())
            {
                if (this.Session().GetFlag(loot.CollectedID)) // 当局已收集
                    loot.RemoveSelf();
            }
        }

        // 恢复本局积分
        curValue = this.Session().GetCounter(CurrentValueID);

        leftTime = this.Session().GetCounter(LeftTimeID);
        if (leftTime == 0)
            leftTime = totalTime;
        else
            leftTime = (float)this.Session().GetCounter(LeftTimeID) / 1000;

        display.Show();
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);
        display.Hide();
    }

    public override void Update()
    {
        base.Update();

        leftTime = Math.Max(0, leftTime - Engine.DeltaTime * timeReduceSpeedMultiplier);
        display.Time = leftTime;

        // if (MInput.Keyboard.Pressed(Keys.Enter))
        this.Session().SetCounter(LeftTimeID, (int)(leftTime * 1000));
        this.Session().SetCounter(CurrentValueID, curValue);
        // 时间结束 或收集完, 返回主大厅
        // 玩家死亡时停止计数
        if (leftTime == 0 || (exitWhenLootsAllCollected && this.GetEntities<Loot>().Count == 0))
        {
            display.Hide();
            // 记录剩余时间
            this.Session().SetCounter(LeftTimeID, (int)(totalTime * 1000));
            // 记录历史最高分
            this.Session().SetCounter(LootMaxCreditsID, Math.Max(curValue, this.Session().GetCounter(LootMaxCreditsID)));

            bool playerAlive = this.GetEntity<Player>() != null;
            Vector2 pos = SceneAs<Level>().Session.MapData.Get(teleportToRoomNameWhenTimeOver).Spawns[0];
            if (playerAlive)
            {
                // 清除player携带的实体
                Leader leader = this.GetEntity<Player>().Get<Leader>();
                this.GetEntities<Loot>().ForEach(loot => leader.LoseFollower(loot.Get<Follower>()));


                // 因为player死亡的时候timer不会继续更新, 所以在timer为0的时候不用担心player为空
                Scene.Tracker.GetEntity<Player>().Position = pos;
            }
            else
            {
                this.Session().RespawnPoint = pos;
            }
        }

        display.Value = curValue;
    }

    public void AddValue(int value)
    {
        curValue += value;
    }
}