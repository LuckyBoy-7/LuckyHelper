## 所有实体一览

由于基本上都是给别人写的东西, 所以会附上灵感作者, 同时也非常感谢反馈 bug 的人🥰

* PassByRefill(Entity): 梦...开始的地方 -- by Saplonily(伟大无需多言)
* SetFallingBlockBlockFloatySpaceBlock(Trigger): 使用后掉落块掉在月亮块上时月亮块会停止, 否则原版会抽搐 -- by Dizer
* Custom Water(Entity): 自定义的水 -- by Molong
* Death Count Text(Entity): 死亡计数器 -- by Molong
* Timer Text(Entity): 计时器 -- by Molong
* Custom Gondola(Entity): 自定义缆车 -- by Breaker-K, 底龙
* Arbitrary Shape Conquest Area(Entity): 灵感来源于战地占点 -- by 键盘英雄(由于好像没人用过, 所以可能会有很多 bug)
* Dream Zone(Entity): 梦境区域, 可看作没有碰撞的果冻 -- by 底龙(Shynnie, AfterDawn, Myn)
* Enable Player Falling Through JumpThru(Settings): 下蹲穿单向板
* Light Source Adjust(Trigger): 调整光源亮度和透明度(本来应该给别人提 pr 的, 但是人家好像好久没更了就还是选择了抄())  -- by Myn
* Freeze Trigger(Trigger): 给冻结帧的 Trigger -- by Myn
* Decal With Combined Registry(Entity): 原版 DecalRegistry 基于固定路径, 这个路径就只是一个 id/key, 可以随意组合 -- by Myn
* Reskinnable Badeline Boost(Entity): 可换皮肤的 Badeline Boost -- by Riki
* TalkComponent Controller(Trigger): 调整对话组件的显隐(就是望远镜上面那个 UI) -- by Shynnie, AfterDawn
* Overlap Pair Set Flag(Trigger): 相交就设置 Flag 的 Trigger -- by Shynnie, AfterDawn
* Fast Bubble Controller(Trigger): 控制是否可以泡泡快启 -- by Riki
* Entity Pinner(Entity): 一个可以吸取一些实体的东西, 比如 Theo水晶, 水母, 玩家等 -- by 底龙
* Speed Redirect(Entity): 进入时可以随意调整玩家的速度方向和大小 -- by 底龙
* Player Movement Controller(Trigger): 调节玩家运动参数(移动/跳跃速度, 加速度之类的, 以后可能还会补充) -- by Molong
* Crystal Heart Dialog Controller(Entity): 为水晶之心单独设置文本 -- by Touchme_uwu
* Set Condition Flag(Trigger): 在玩家做出某种行为时设置 flag -- by Shynnie
* Invert Flag(Trigger): 反转某个 flag
* Logic Flag(Trigger): 根据逻辑判断是否设置 flag, 比如如果条件 (flag1 && (flag2 || !flag3)) 成立, 则设置对应 flag
* FollowerContainer(Entity): 类 Eevee 的跟随容器, 可以像草莓那样被收集 -- by Shynnie(抄了 [Eevee](https://github.com/CommunalHelper/EeveeHelper)代码, 原谅我😭)
* DetachFollowerContainer(Trigger): 可以解绑 FollowerContainer -- by Shynnie
* AudioAdjust(Trigger): 调整特定音频的音量(其他属性等有需求了再说) -- by NaCline, 底龙
* ColorModifier(Entity): 改实体颜色的
* OrderedFlag(Trigger): 按顺序触发 flag -- by Shynnie
* ToggleOrbitContainer(Entity): 框选一部分实体沿着圆周运动, 可用 flag 操控并受风的影响 -- by ShadowRo
* CameraUpdateHelper(Trigger): 控制摄像机部分的运动 -- by NaCline
* PlayerInvincibleController(Trigger): 设置玩家在某些情况下的无敌状态, 比如在碰到刺的时候冲刺瞬间不会死 -- by ShadowRo
* MenuButtonController(Entity): 开启某些 flag 后可以禁用 pause 菜单中的特定 button -- by ShadowRo
* MoveContainer(Entity): 更高级的 Flag Mover(也许
* QuantumContainer(Entity): 类似 ow 里的量子碎片
* Ball(Entity): 球 -- by 底龙
* AtlasPathReplacer(Entity): 替换 Atlas 中的一些硬编码 -- by Myn, 底龙
* LinkedWire(Entity): 可以相互链接的 Wire -- by 底龙
* DummyPlayer(Entity): Player 木偶, 用来触发各种 Trigger -- by Riki(云雀)
* PasteRoom(Entity): 粘贴房间实体 
* CopyItem(Trigger): 复制 entity, trigger, decal 的实体 
* PasteItem(Entity): 粘贴 CopyItem 复制的东西 
* PasteItemDuplicator(Trigger): 使用 PasteItem 来等距的生成多个复制品(小豹猫awa 114514 系列😱)
* CrackAdder(Entity): 自动为砖添加裂纹 -- by 北极星(Nacline) 
* AudioPlay(Trigger): 播放音效 -- (懒得开 Helper 导致的) 
* DisperseSprites(Trigger): 消散对象(剧情向) -- 云起时 

### GhostTranspose -- by Molong

* GhostTranpose(Trigger): 像 iwannna 中的 nang s9 一样可以发射幻影传送到碰撞位置
* GhostTranspose Barrier(Entity): 一堵阻挡幻影的墙, 玩家可以自由穿过
* Kill Ghost(Trigger): 清除幻影的 Trigger

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

## 脚本 Scripts

* flip room: 反转房间/房间内的实体

## 致谢

我感谢天, 感谢地, 感谢你:

* Saplonily
* WEGFan(伟哥基)
* wanmaple(浣熊)
* Lozen(反向钟)
* Dizer(谛泽)
* Molong
* Breaker-K
* UnderDragon(底龙)
* jpyx258(键盘英雄)
* Myn_Gen(没有你)
* SSM24
* Appels
* paperlock(单个标点符号)
* ABuffZucchini
* RikiUwU
* kuksa
* nocolm
* Shynnie(Shy 酱)
* AfterDawn_Cxwg(晨曦微光)
* youzi(柚子)
* Touchme_uwu(ΓΠ)
* 陌生的下界合金剑
* 茶杯
* SunsetQuasar
* ShadowRo
* Wartori
* Nacline
* Deep Blue Berry(深蓝草莓)
* JaThePlayer
* limia | amber leaf bluffs
* orig_Snip
* Luke870
* Nope208
* ella-TAS
* Butcherberries
* Maddie
* 云雀
* 电箱
* 小豹猫awa
* 云起时