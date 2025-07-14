## 所有实体一览

由于基本上都是给别人写的东西, 所以会附上灵感作者

* PassByRefill(Entity): 梦...开始的地方 -- by Saplonily
* SetFallingBlockBlockFloatySpaceBlock(Trigger): 使用后掉落块掉在月亮块上时月亮块会停止, 否则原版会抽搐 -- by Dizer
* Custom Water(Entity): 自定义的水 -- by Molong
* Death Count Text(Entity): 死亡计数器 -- by Molong
* Timer Text(Entity): 计时器 -- by Molong
* CustomGondola(Entity): 自定义缆车 -- by Breaker-K, 底龙
* ArbitraryShapeConquestArea(Entity): 灵感来源于战地占点 -- by 键盘英雄(由于好像没人用过, 所以可能会有很多 bug)
* DreamZone(Entity): 梦境区域, 可看作没有碰撞的果冻 -- by 底龙
* EnablePlayerFallingThroughJumpThru(Settings): 下蹲穿单向板
* LightSourceAdjust(Trigger): 调整光源亮度和透明度(本来应该给别人提 pr 的, 但是人家好像好久没更了就还是选择了抄())  -- by Myn
* FreezeTrigger(Trigger): 给冻结帧的 Trigger -- by Myn
* DecalWithCombinedRegistry(Entity): 原版 DecalRegistry 基于固定路径, 这个路径就只是一个 id/key, 可以随意组合 -- by Myn
* ReskinnableBadelineBoost(Entity): 可换皮肤的 Badeline Boost -- by Riki
* TalkComponentController(Trigger): 调整对话组件的显隐(就是望远镜上面那个 UI) -- by Shynnie, AfterDawn
* OverlapPairSetFlag(Trigger): 相交就设置 Flag 的 Trigger -- by Shynnie, AfterDawn
* FastBubbleController(Trigger): 控制是否可以泡泡快启 -- by Riki
* EntityPinner(Entity): 一个可以吸取一些实体的东西, 比如 Theo水晶, 水母, 玩家等 -- by 底龙
* SpeedRedirect(Entity): 进入时可以随意调整玩家的速度方向和大小 -- by 底龙

### GhostTranspose -- by Molong

* GhostTranposeTrigger(Trigger): 像 iwannna 中的 nang s9 一样可以发射幻影传送到碰撞位置
* GhostTransposeBarrier(Entity): 一堵阻挡幻影的墙, 玩家可以自由穿过
* KillGhostTrigger(Trigger): 清除幻影的 Trigger

### LootSpeedrun -- by 键盘英雄(由于好像没人用过, 所以可能会有很多 bug)

~~又想删掉黑历史又怕有人正在用 be like~~

This is a mini game about looting loots spread among the rooms in limited time, when time is over, you'll be killed and respawn in the hub(where the 'LootSpeedrunController' is
placed), To use these stuff, here's instruction:

You have to create a room as hub, put 'LootSpeedrunController' in the hub to set basic params about the mini game, then move it close to a respawnPoint(Player), which will be
regarded by 'LootSpeedrunController' as a returnPoint when time is over, then put 'LootSpeedrunStartup' in the hub and put 'Loot' and 'LootCollectArea' in other rooms. That's all.

* Loot(Entity): An entity like strawberry but with a value field and only works in this mini game
* LootCollectArea(Trigger): Loot can only be collected when player touches this
* LootInfoVisibleSet(Trigger): To show or hide texts of timer and looted value
* LootMaxValueText(Entity): An text to show the max looted value
* LootSpeedrunController(Entity): Set basic info about this loot speedrun like time or time reduce speed, and will find the closest spawn point as a return point when time is over
* LootSpeedrunInfoDisplay(Entity, but instantiated by LootSpeedrunController): Show left time and gained values
* LootSpeedrunStartup(Trigger): The entrance of this loot speedrun, it will start this mini game on touching player
* LootThresholdFlagSet(Trigger): When you gain a loot , you gain some value, when value is equal or greater than the threshold, the flag will be set

## Metadata

在地图文件旁创建与地图同名的`.meta.yaml`文件, 可以单独为地图配置一些属性

### 自定义默认文本框 -- by 柚子

```yaml
LuckyHelperAreaMetaData:
    DefaultTextboxPath: "textbox/LuckyHelper/default"
    DefaultMiniTextboxPath: "textbox/LuckyHelper/default_mini"
```