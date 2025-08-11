using Celeste.Mod.Entities;
using LuckyHelper.Extensions;
using Microsoft.Xna.Framework.Input;

namespace LuckyHelper.Entities.LootSpeedrun;

[CustomEntity("LuckyHelper/LootSpeedrunController")]
[Tracked]
public class LootSpeedrunController : Entity
{
    public const string LootMaxValueID = "LootMaxValue";
    private LootSpeedrunInfoDisplay display;

    private float leftTime = 0;

    // private string ID => $"{this.Session().LevelData.Name}_LootSpeedrunController";
    // private string LeftTimeID => ID + "_LeftTime";
    // private string CurrentValueID => ID + "_CurrentValue";
    public int CurValue;
    private bool hasStarted;
    private Vector2 returnPoint;
    private string startRoom;

    // -----------------------------------------------------------------------
    private float totalTime = 100;
    private float timeReduceSpeedMultiplier = 5;


    public LootSpeedrunController(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        Tag = Tags.Global;
        // 保证最后更新最后, 删除restart flag
        Depth = -100000000;
        totalTime = data.Float("totalTime");
        timeReduceSpeedMultiplier = data.Float("timeReduceSpeedMultiplier");

        leftTime = totalTime;
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        var first = this.GetEntity<LootSpeedrunController>();
        if (first != this) // 如果有别的了,删除自己保持单例
        {
            RemoveSelf();
            return;
        }

        returnPoint = this.Session().GetSpawnPoint(Position);
        startRoom = this.Session().Level;
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        if (Scene.Tracker.GetEntity<LootSpeedrunInfoDisplay>() == null)
            Scene.Add(display = new LootSpeedrunInfoDisplay());
        display ??= Scene.Tracker.GetEntity<LootSpeedrunInfoDisplay>();


        // // 恢复本局积分
        // curValue = this.Session().GetCounter(CurrentValueID);
        //
        // leftTime = this.Session().GetCounter(LeftTimeID);
        // if (leftTime == 0)
        //     leftTime = totalTime;
        // else
        //     leftTime = (float)this.Session().GetCounter(LeftTimeID) / 1000;
    }

    public void TryStart()
    {
        if (hasStarted)
            return;
        // 新的一局
        foreach (var levelData in this.Session().MapData.Levels)
        {
            foreach (var entityData in levelData.Entities)
            {
                if (entityData.Name == "LuckyHelper/Loot")
                {
                    this.Session().SetFlag(entityData.ID + "_collected", false);
                }
            }
        }

        leftTime = totalTime;
        CurValue = 0;
        hasStarted = true;

        display.Show();
    }

    public override void Update()
    {
        if (MInput.Keyboard.Pressed(Keys.Enter))
        {
            OnTimeOver();
        }


        base.Update();
        if (!hasStarted)
            return;

        if (this.GetEntity<Player>() != null)
            leftTime = Math.Max(0, leftTime - Engine.DeltaTime * timeReduceSpeedMultiplier);
        display.Time = leftTime;

        // 时间结束 或收集完, 返回主大厅
        if (leftTime == 0)
        {
            OnTimeOver();
        }

        // 记录历史最高分
        this.Session().SetCounter(LootMaxValueID, Math.Max(CurValue, this.Session().GetCounter(LootMaxValueID)));
        display.Value = CurValue;
    }

    private void OnTimeOver()
    {
        hasStarted = false;
        PlayerDeadBody body;
        if (this.GetEntity<Player>() != null)
            body = this.GetEntity<Player>().Die(Vector2.Zero);
        else
            body = this.GetEntity<PlayerDeadBody>();
        body.DeathAction += () =>
        {
            this.Session().Level = startRoom;
            this.Session().RespawnPoint = returnPoint;
            this.Level().Reload();
        };
    }
}