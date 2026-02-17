using LuckyHelper.Modules;

namespace LuckyHelper.Extensions;

public static class LevelExtensions
{
    public static void AddEntityWithEntityData(this Level orig, EntityData entityData)
    {
        List<EntityID> dontLoadEntityWithID = new List<EntityID>();
        Player entity = orig.Tracker.GetEntity<Player>();
        if (entity != null)
        {
            foreach (Follower follower in entity.Leader.Followers)
            {
                dontLoadEntityWithID.Add(follower.ParentEntityID);
            }
        }

        LevelData levelData = orig.Session.LevelData;
        Vector2 levelOffset = new Vector2(levelData.Bounds.Left, levelData.Bounds.Top);
        EntityID entityID = new EntityID(levelData.Name, entityData.ID);
        if (orig.Session.DoNotLoad.Contains(entityID) || dontLoadEntityWithID.Contains(entityID))
        {
            return;
        }

        switch ((!Level.LoadCustomEntity(entityData, orig)) ? entityData.Name : "")
        {
            case "checkpoint":
                // if (flag3)
                // {
                //  Checkpoint checkpoint = new Checkpoint(entityData, handledLevelOffset);
                //  orig.Add(checkpoint);
                //  startPosition = entityData.Position + handledLevelOffset + checkpoint.SpawnOffset;
                // }

                break;
            case "jumpThru":
                orig.Add(new JumpthruPlatform(entityData, levelOffset));
                break;
            case "refill":
                orig.Add(new Refill(entityData, levelOffset));
                break;
            case "infiniteStar":
                orig.Add(new FlyFeather(entityData, levelOffset));
                break;
            case "strawberry":
                orig.Add(new Strawberry(entityData, levelOffset, entityID));
                break;
            case "memorialTextController":
                if (orig.Session.Dashes == 0 && (orig.Session.StartedFromBeginning || orig.Session.RestartedFromGolden))
                {
                    orig.Add(new Strawberry(entityData, levelOffset, entityID));
                }

                break;
            case "goldenBerry":
            {
                bool cheatMode = SaveData.Instance.CheatMode;
                bool flag4 = orig.Session.FurthestSeenLevel == orig.Session.Level || orig.Session.Deaths == 0;
                bool flag5 = SaveData.Instance.UnlockedModes >= 3 || SaveData.Instance.DebugMode;
                bool completed = SaveData.Instance.Areas_Safe[orig.Session.Area.ID].Modes[(int)orig.Session.Area.Mode].Completed;
                if ((cheatMode || (flag5 && completed)) && flag4)
                {
                    orig.Add(new Strawberry(entityData, levelOffset, entityID));
                }

                break;
            }
            case "summitgem":
                orig.Add(new SummitGem(entityData, levelOffset, entityID));
                break;
            case "blackGem":
                if (!orig.Session.HeartGem || orig._PatchHeartGemBehavior(orig.Session.Area.Mode) != AreaMode.Normal)
                {
                    orig.Add(new HeartGem(entityData, levelOffset));
                }

                break;
            case "dreamHeartGem":
                if (!orig.Session.HeartGem)
                {
                    orig.Add(new DreamHeartGem(entityData, levelOffset));
                }

                break;
            case "spring":
                orig.Add(new Spring(entityData, levelOffset, Spring.Orientations.Floor));
                break;
            case "wallSpringLeft":
                orig.Add(new Spring(entityData, levelOffset, Spring.Orientations.WallLeft));
                break;
            case "wallSpringRight":
                orig.Add(new Spring(entityData, levelOffset, Spring.Orientations.WallRight));
                break;
            case "fallingBlock":
                orig.Add(new FallingBlock(entityData, levelOffset));
                break;
            case "zipMover":
                orig.Add(new ZipMover(entityData, levelOffset));
                break;
            case "crumbleBlock":
                orig.Add(new CrumblePlatform(entityData, levelOffset));
                break;
            case "dreamBlock":
                orig.Add(new DreamBlock(entityData, levelOffset));
                break;
            case "touchSwitch":
                orig.Add(new TouchSwitch(entityData, levelOffset));
                break;
            case "switchGate":
                orig.Add(new SwitchGate(entityData, levelOffset));
                break;
            case "negaBlock":
                orig.Add(new NegaBlock(entityData, levelOffset));
                break;
            case "key":
                orig.Add(new Key(entityData, levelOffset, entityID));
                break;
            case "lockBlock":
                orig.Add(new LockBlock(entityData, levelOffset, entityID));
                break;
            case "movingPlatform":
                orig.Add(new MovingPlatform(entityData, levelOffset));
                break;
            case "rotatingPlatforms":
            {
                Vector2 handledLevelOffset2 = entityData.Position + levelOffset;
                Vector2 handledLevelOffset3 = entityData.Nodes[0] + levelOffset;
                int width = entityData.Width;
                int num2 = entityData.Int("platforms");
                bool clockwise = entityData.Bool("clockwise");
                float length = (handledLevelOffset2 - handledLevelOffset3).Length();
                float num3 = (handledLevelOffset2 - handledLevelOffset3).Angle();
                float num4 = MathF.PI * 2f / (float)num2;
                for (int j = 0; j < num2; j++)
                {
                    float angleRadians = num3 + num4 * (float)j;
                    angleRadians = Calc.WrapAngle(angleRadians);
                    Vector2 position2 = handledLevelOffset3 + Calc.AngleToVector(angleRadians, length);
                    orig.Add(new RotatingPlatform(position2, width, handledLevelOffset3, clockwise));
                }

                break;
            }
            case "blockField":
                orig.Add(new BlockField(entityData, levelOffset));
                break;
            case "cloud":
                orig.Add(new Cloud(entityData, levelOffset));
                break;
            case "booster":
                orig.Add(new Booster(entityData, levelOffset));
                break;
            case "moveBlock":
                orig.Add(new MoveBlock(entityData, levelOffset));
                break;
            case "light":
                orig.Add(new PropLight(entityData, levelOffset));
                break;
            case "switchBlock":
            case "swapBlock":
                orig.Add(new SwapBlock(entityData, levelOffset));
                break;
            case "dashSwitchH":
            case "dashSwitchV":
                orig.Add(DashSwitch.Create(entityData, levelOffset, entityID));
                break;
            case "templeGate":
                orig.Add(new TempleGate(entityData, levelOffset, levelData.Name));
                break;
            case "torch":
                orig.Add(new Torch(entityData, levelOffset, entityID));
                break;
            case "templeCrackedBlock":
                orig.Add(new TempleCrackedBlock(entityID, entityData, levelOffset));
                break;
            case "seekerBarrier":
                orig.Add(new SeekerBarrier(entityData, levelOffset));
                break;
            case "theoCrystal":
                orig.Add(new TheoCrystal(entityData, levelOffset));
                break;
            case "glider":
                orig.Add(new Glider(entityData, levelOffset));
                break;
            case "theoCrystalPedestal":
                orig.Add(new TheoCrystalPedestal(entityData, levelOffset));
                break;
            case "badelineBoost":
                orig.Add(new BadelineBoost(entityData, levelOffset));
                break;
            case "cassette":
                if (!orig.Session.Cassette)
                {
                    orig.Add(new Cassette(entityData, levelOffset));
                }

                break;
            case "cassetteBlock":
            {
                CassetteBlock cassetteBlock = new CassetteBlock(entityData, levelOffset, entityID);
                orig.Add(cassetteBlock);
                orig.HasCassetteBlocks = true;
                if (orig.CassetteBlockTempo == 1f)
                {
                    orig.CassetteBlockTempo = cassetteBlock.Tempo;
                }

                orig.CassetteBlockBeats = Math.Max(cassetteBlock.Index + 1, orig.CassetteBlockBeats);

                // 如果我们有磁带要复制, 这意味着已经有 CassetteBlockManager 了
                // if (!flag)
                // {
                //  flag = true;
                //  if (orig.Tracker.GetEntity<CassetteBlockManager>() == null && orig.ShouldCreateCassetteManager)
                //  {
                //   orig.Add(new CassetteBlockManager());
                //  }
                // }

                break;
            }
            case "wallBooster":
                orig.Add(new WallBooster(entityData, levelOffset));
                break;
            case "bounceBlock":
                orig.Add(new BounceBlock(entityData, levelOffset));
                break;
            case "coreModeToggle":
                orig.Add(new CoreModeToggle(entityData, levelOffset));
                break;
            case "iceBlock":
                orig.Add(new IceBlock(entityData, levelOffset));
                break;
            case "fireBarrier":
                orig.Add(new FireBarrier(entityData, levelOffset));
                break;
            case "eyebomb":
                orig.Add(new Puffer(entityData, levelOffset));
                break;
            case "flingBird":
                orig.Add(new FlingBird(entityData, levelOffset));
                break;
            case "flingBirdIntro":
                orig.Add(new FlingBirdIntro(entityData, levelOffset));
                break;
            case "birdPath":
                orig.Add(new BirdPath(entityID, entityData, levelOffset));
                break;
            case "lightningBlock":
                orig.Add(new LightningBreakerBox(entityData, levelOffset));
                break;
            case "spikesUp":
                orig.Add(new Spikes(entityData, levelOffset, Spikes.Directions.Up));
                break;
            case "spikesDown":
                orig.Add(new Spikes(entityData, levelOffset, Spikes.Directions.Down));
                break;
            case "spikesLeft":
                orig.Add(new Spikes(entityData, levelOffset, Spikes.Directions.Left));
                break;
            case "spikesRight":
                orig.Add(new Spikes(entityData, levelOffset, Spikes.Directions.Right));
                break;
            case "triggerSpikesUp":
                orig.Add(new TriggerSpikes(entityData, levelOffset, TriggerSpikes.Directions.Up));
                break;
            case "triggerSpikesDown":
                orig.Add(new TriggerSpikes(entityData, levelOffset, TriggerSpikes.Directions.Down));
                break;
            case "triggerSpikesRight":
                orig.Add(new TriggerSpikes(entityData, levelOffset, TriggerSpikes.Directions.Right));
                break;
            case "triggerSpikesLeft":
                orig.Add(new TriggerSpikes(entityData, levelOffset, TriggerSpikes.Directions.Left));
                break;
            case "darkChaser":
                orig.Add(new BadelineOldsite(entityData, levelOffset, 0));
                // orig.Add(new BadelineOldsite(entityData, handledLevelOffset, num));
                // num++;
                break;
            case "rotateSpinner":
                if (orig.Session.Area.ID == 10)
                {
                    orig.Add(new StarRotateSpinner(entityData, levelOffset));
                }
                else if (orig.Session.Area.ID == 3 || (orig.Session.Area.ID == 7 && orig.Session.Level.StartsWith("d-")))
                {
                    orig.Add(new DustRotateSpinner(entityData, levelOffset));
                }
                else
                {
                    orig.Add(new BladeRotateSpinner(entityData, levelOffset));
                }

                break;
            case "trackSpinner":
                if (orig.Session.Area.ID == 10)
                {
                    orig.Add(new StarTrackSpinner(entityData, levelOffset));
                }
                else if (orig.Session.Area.ID == 3 || (orig.Session.Area.ID == 7 && orig.Session.Level.StartsWith("d-")))
                {
                    orig.Add(new DustTrackSpinner(entityData, levelOffset));
                }
                else
                {
                    orig.Add(new BladeTrackSpinner(entityData, levelOffset));
                }

                break;
            case "spinner":
            {
                if (orig.Session.Area.ID == 3 || (orig.Session.Area.ID == 7 && orig.Session.Level.StartsWith("d-")))
                {
                    orig.Add(new DustStaticSpinner(entityData, levelOffset));
                    break;
                }

                CrystalColor color = CrystalColor.Blue;
                if (orig.Session.Area.ID == 5)
                {
                    color = CrystalColor.Red;
                }
                else if (orig.Session.Area.ID == 6)
                {
                    color = CrystalColor.Purple;
                }
                else if (orig.Session.Area.ID == 10)
                {
                    color = CrystalColor.Rainbow;
                }

                orig.Add(new CrystalStaticSpinner(entityData, levelOffset, color));
                break;
            }
            case "sinkingPlatform":
                orig.Add(new SinkingPlatform(entityData, levelOffset));
                break;
            case "friendlyGhost":
                orig.Add(new AngryOshiro(entityData, levelOffset));
                break;
            case "seeker":
                orig.Add(new Seeker(entityData, levelOffset));
                break;
            case "seekerStatue":
                orig.Add(new SeekerStatue(entityData, levelOffset));
                break;
            case "slider":
                orig.Add(new Slider(entityData, levelOffset));
                break;
            case "templeBigEyeball":
                orig.Add(new TempleBigEyeball(entityData, levelOffset));
                break;
            case "crushBlock":
                orig.Add(new CrushBlock(entityData, levelOffset));
                break;
            case "bigSpinner":
                orig.Add(new Bumper(entityData, levelOffset));
                break;
            case "starJumpBlock":
                orig.Add(new StarJumpBlock(entityData, levelOffset));
                break;
            case "floatySpaceBlock":
                orig.Add(new FloatySpaceBlock(entityData, levelOffset));
                break;
            case "glassBlock":
                orig.Add(new GlassBlock(entityData, levelOffset));
                break;
            case "goldenBlock":
                orig.Add(new GoldenBlock(entityData, levelOffset));
                break;
            case "fireBall":
                orig.Add(new FireBall(entityData, levelOffset));
                break;
            case "risingLava":
                orig.Add(new RisingLava(entityData, levelOffset));
                break;
            case "sandwichLava":
                orig.Add(new SandwichLava(entityData, levelOffset));
                break;
            case "killbox":
                orig.Add(new Killbox(entityData, levelOffset));
                break;
            case "fakeHeart":
                orig.Add(new FakeHeart(entityData, levelOffset));
                break;
            case "lightning":
                if (entityData.Bool("perLevel") || !orig.Session.GetFlag("disable_lightning"))
                {
                    orig.Add(new Lightning(entityData, levelOffset));
                    // flag2 = true;
                }

                break;
            case "finalBoss":
                orig.Add(new FinalBoss(entityData, levelOffset));
                break;
            case "finalBossFallingBlock":
                orig.Add(FallingBlock.CreateFinalBossBlock(entityData, levelOffset));
                break;
            case "finalBossMovingBlock":
                orig.Add(new FinalBossMovingBlock(entityData, levelOffset));
                break;
            case "fakeWall":
                orig.Add(new FakeWall(entityID, entityData, levelOffset, FakeWall.Modes.Wall));
                break;
            case "fakeBlock":
                orig.Add(new FakeWall(entityID, entityData, levelOffset, FakeWall.Modes.Block));
                break;
            case "dashBlock":
                orig.Add(new DashBlock(entityData, levelOffset, entityID));
                break;
            case "invisibleBarrier":
                orig.Add(new InvisibleBarrier(entityData, levelOffset));
                break;
            case "exitBlock":
                orig.Add(new ExitBlock(entityData, levelOffset));
                break;
            case "conditionBlock":
            {
                Level.ConditionBlockModes conditionBlockModes = entityData.Enum("condition", Level.ConditionBlockModes.Key);
                EntityID none = EntityID.None;
                string[] array = entityData.Attr("conditionID").Split(new char[1] { ':' });
                none.Level = array[0];
                none.ID = Convert.ToInt32(array[1]);
                if (conditionBlockModes switch
                    {
                        Level.ConditionBlockModes.Button => orig.Session.GetFlag(DashSwitch.GetFlagName(none)),
                        Level.ConditionBlockModes.Key => orig.Session.DoNotLoad.Contains(none),
                        Level.ConditionBlockModes.Strawberry => orig.Session.Strawberries.Contains(none),
                        _ => throw new Exception("Condition type not supported!"),
                    })
                {
                    orig.Add(new ExitBlock(entityData, levelOffset));
                }

                break;
            }
            case "coverupWall":
                orig.Add(new CoverupWall(entityData, levelOffset));
                break;
            case "crumbleWallOnRumble":
                orig.Add(new CrumbleWallOnRumble(entityData, levelOffset, entityID));
                break;
            case "ridgeGate":
                if (orig.GotCollectables(entityData))
                {
                    orig.Add(new RidgeGate(entityData, levelOffset));
                }

                break;
            case "tentacles":
                orig.Add(new ReflectionTentacles(entityData, levelOffset));
                break;
            case "starClimbController":
                orig.Add(new StarJumpController());
                break;
            case "playerSeeker":
                orig.Add(new PlayerSeeker(entityData, levelOffset));
                break;
            case "chaserBarrier":
                orig.Add(new ChaserBarrier(entityData, levelOffset));
                break;
            case "introCrusher":
                orig.Add(new IntroCrusher(entityData, levelOffset));
                break;
            case "bridge":
                orig.Add(new Bridge(entityData, levelOffset));
                break;
            case "bridgeFixed":
                orig.Add(new BridgeFixed(entityData, levelOffset));
                break;
            case "bird":
                orig.Add(new BirdNPC(entityData, levelOffset));
                break;
            case "introCar":
                orig.Add(new IntroCar(entityData, levelOffset));
                break;
            case "memorial":
                orig.Add(new Memorial(entityData, levelOffset));
                break;
            case "wire":
                orig.Add(new Wire(entityData, levelOffset));
                break;
            case "cobweb":
                orig.Add(new Cobweb(entityData, levelOffset));
                break;
            case "lamp":
                orig.Add(new Lamp(levelOffset + entityData.Position, entityData.Bool("broken")));
                break;
            case "hanginglamp":
                orig.Add(new HangingLamp(entityData, levelOffset + entityData.Position));
                break;
            case "hahaha":
                orig.Add(new Hahaha(entityData, levelOffset));
                break;
            case "bonfire":
                orig.Add(new Bonfire(entityData, levelOffset));
                break;
            case "payphone":
                orig.Add(new Payphone(levelOffset + entityData.Position));
                break;
            case "colorSwitch":
                orig.Add(new ClutterSwitch(entityData, levelOffset));
                break;
            case "clutterDoor":
                orig.Add(new ClutterDoor(entityData, levelOffset, orig.Session));
                break;
            case "dreammirror":
                orig.Add(new DreamMirror(levelOffset + entityData.Position));
                break;
            case "resortmirror":
                orig.Add(new ResortMirror(entityData, levelOffset));
                break;
            case "towerviewer":
                orig.Add(new Lookout(entityData, levelOffset));
                break;
            case "picoconsole":
                orig.Add(new PicoConsole(entityData, levelOffset));
                break;
            case "wavedashmachine":
                orig.Add(new WaveDashTutorialMachine(entityData, levelOffset));
                break;
            case "yellowBlocks":
                ClutterBlockGenerator.Init(orig);
                ClutterBlockGenerator.Add((int)(entityData.Position.X / 8f), (int)(entityData.Position.Y / 8f), entityData.Width / 8, entityData.Height / 8,
                    ClutterBlock.Colors.Yellow);
                break;
            case "redBlocks":
                ClutterBlockGenerator.Init(orig);
                ClutterBlockGenerator.Add((int)(entityData.Position.X / 8f), (int)(entityData.Position.Y / 8f), entityData.Width / 8, entityData.Height / 8,
                    ClutterBlock.Colors.Red);
                break;
            case "greenBlocks":
                ClutterBlockGenerator.Init(orig);
                ClutterBlockGenerator.Add((int)(entityData.Position.X / 8f), (int)(entityData.Position.Y / 8f), entityData.Width / 8, entityData.Height / 8,
                    ClutterBlock.Colors.Green);
                break;
            case "oshirodoor":
                orig.Add(new MrOshiroDoor(entityData, levelOffset));
                break;
            case "templeMirrorPortal":
                orig.Add(new TempleMirrorPortal(entityData, levelOffset));
                break;
            case "reflectionHeartStatue":
                orig.Add(new ReflectionHeartStatue(entityData, levelOffset));
                break;
            case "resortRoofEnding":
                orig.Add(new ResortRoofEnding(entityData, levelOffset));
                break;
            case "gondola":
                orig.Add(new Gondola(entityData, levelOffset));
                break;
            case "birdForsakenCityGem":
                orig.Add(new ForsakenCitySatellite(entityData, levelOffset));
                break;
            case "whiteblock":
                orig.Add(new WhiteBlock(entityData, levelOffset));
                break;
            case "plateau":
                orig.Add(new Plateau(entityData, levelOffset));
                break;
            case "soundSource":
                orig.Add(new SoundSourceEntity(entityData, levelOffset));
                break;
            case "templeMirror":
                orig.Add(new TempleMirror(entityData, levelOffset));
                break;
            case "templeEye":
                orig.Add(new TempleEye(entityData, levelOffset));
                break;
            case "clutterCabinet":
                orig.Add(new ClutterCabinet(entityData, levelOffset));
                break;
            case "floatingDebris":
                orig.Add(new FloatingDebris(entityData, levelOffset));
                break;
            case "foregroundDebris":
                orig.Add(new ForegroundDebris(entityData, levelOffset));
                break;
            case "moonCreature":
                orig.Add(new MoonCreature(entityData, levelOffset));
                break;
            case "lightbeam":
                orig.Add(new LightBeam(entityData, levelOffset));
                break;
            case "door":
                orig.Add(new Door(entityData, levelOffset));
                break;
            case "trapdoor":
                orig.Add(new Trapdoor(entityData, levelOffset));
                break;
            case "resortLantern":
                orig.Add(new ResortLantern(entityData, levelOffset));
                break;
            case "water":
                orig.Add(new Water(entityData, levelOffset));
                break;
            case "waterfall":
                orig.Add(new WaterFall(entityData, levelOffset));
                break;
            case "bigWaterfall":
                orig.Add(new BigWaterfall(entityData, levelOffset));
                break;
            case "clothesline":
                orig.Add(new Clothesline(entityData, levelOffset));
                break;
            case "cliffflag":
                orig.Add(new CliffFlags(entityData, levelOffset));
                break;
            case "cliffside_flag":
                orig.Add(new CliffsideWindFlag(entityData, levelOffset));
                break;
            case "flutterbird":
                orig.Add(new FlutterBird(entityData, levelOffset));
                break;
            case "SoundTest3d":
                orig.Add(new _3dSoundTest(entityData, levelOffset));
                break;
            case "SummitBackgroundManager":
                orig.Add(new AscendManager(entityData, levelOffset));
                break;
            case "summitGemManager":
                orig.Add(new SummitGemManager(entityData, levelOffset));
                break;
            case "heartGemDoor":
                orig.Add(new HeartGemDoor(entityData, levelOffset));
                break;
            case "summitcheckpoint":
                orig.Add(new SummitCheckpoint(entityData, levelOffset));
                break;
            case "summitcloud":
                orig.Add(new SummitCloud(entityData, levelOffset));
                break;
            case "coreMessage":
                orig.Add(new CoreMessage(entityData, levelOffset));
                break;
            case "playbackTutorial":
                orig.Add(new PlayerPlayback(entityData, levelOffset));
                break;
            case "playbackBillboard":
                orig.Add(new PlaybackBillboard(entityData, levelOffset));
                break;
            case "cutsceneNode":
                orig.Add(new CutsceneNode(entityData, levelOffset));
                break;
            case "kevins_pc":
                orig.Add(new KevinsPC(entityData, levelOffset));
                break;
            case "powerSourceNumber":
                orig.Add(new PowerSourceNumber(entityData.Position + levelOffset, entityData.Int("number", 1), orig.GotCollectables(entityData)));
                break;
            case "npc":
            {
                string text = entityData.Attr("npc").ToLower();
                Vector2 position = entityData.Position + levelOffset;
                switch (text)
                {
                    case "granny_00_house":
                        orig.Add(new NPC00_Granny(position));
                        break;
                    case "theo_01_campfire":
                        orig.Add(new NPC01_Theo(position));
                        break;
                    case "theo_02_campfire":
                        orig.Add(new NPC02_Theo(position));
                        break;
                    case "theo_03_escaping":
                        if (!orig.Session.GetFlag("resort_theo"))
                        {
                            orig.Add(new NPC03_Theo_Escaping(position));
                        }

                        break;
                    case "theo_03_vents":
                        orig.Add(new NPC03_Theo_Vents(position));
                        break;
                    case "oshiro_03_lobby":
                        orig.Add(new NPC03_Oshiro_Lobby(position));
                        break;
                    case "oshiro_03_hallway":
                        orig.Add(new NPC03_Oshiro_Hallway1(position));
                        break;
                    case "oshiro_03_hallway2":
                        orig.Add(new NPC03_Oshiro_Hallway2(position));
                        break;
                    case "oshiro_03_bigroom":
                        orig.Add(new NPC03_Oshiro_Cluttter(entityData, levelOffset));
                        break;
                    case "oshiro_03_breakdown":
                        orig.Add(new NPC03_Oshiro_Breakdown(position));
                        break;
                    case "oshiro_03_suite":
                        orig.Add(new NPC03_Oshiro_Suite(position));
                        break;
                    case "oshiro_03_rooftop":
                        orig.Add(new NPC03_Oshiro_Rooftop(position));
                        break;
                    case "granny_04_cliffside":
                        orig.Add(new NPC04_Granny(position));
                        break;
                    case "theo_04_cliffside":
                        orig.Add(new NPC04_Theo(position));
                        break;
                    case "theo_05_entrance":
                        orig.Add(new NPC05_Theo_Entrance(position));
                        break;
                    case "theo_05_inmirror":
                        orig.Add(new NPC05_Theo_Mirror(position));
                        break;
                    case "evil_05":
                        orig.Add(new NPC05_Badeline(entityData, levelOffset));
                        break;
                    case "theo_06_plateau":
                        orig.Add(new NPC06_Theo_Plateau(entityData, levelOffset));
                        break;
                    case "granny_06_intro":
                        orig.Add(new NPC06_Granny(entityData, levelOffset));
                        break;
                    case "badeline_06_crying":
                        orig.Add(new NPC06_Badeline_Crying(entityData, levelOffset));
                        break;
                    case "granny_06_ending":
                        orig.Add(new NPC06_Granny_Ending(entityData, levelOffset));
                        break;
                    case "theo_06_ending":
                        orig.Add(new NPC06_Theo_Ending(entityData, levelOffset));
                        break;
                    case "granny_07x":
                        orig.Add(new NPC07X_Granny_Ending(entityData, levelOffset));
                        break;
                    case "theo_08_inside":
                        orig.Add(new NPC08_Theo(entityData, levelOffset));
                        break;
                    case "granny_08_inside":
                        orig.Add(new NPC08_Granny(entityData, levelOffset));
                        break;
                    case "granny_09_outside":
                        orig.Add(new NPC09_Granny_Outside(entityData, levelOffset));
                        break;
                    case "granny_09_inside":
                        orig.Add(new NPC09_Granny_Inside(entityData, levelOffset));
                        break;
                    case "gravestone_10":
                        orig.Add(new NPC10_Gravestone(entityData, levelOffset));
                        break;
                    case "granny_10_never":
                        orig.Add(new NPC07X_Granny_Ending(entityData, levelOffset, ch9EasterEgg: true));
                        break;
                }

                break;
            }
        }
    }

    public static void AddEntityWithTriggerData(this Level orig, EntityData triggerData)
    {
        LevelData levelData = orig.Session.LevelData;
        Vector2 levelOffset = new Vector2(levelData.Bounds.Left, levelData.Bounds.Top);


        EntityID entityID = new EntityID(levelData.Name, triggerData.ID + 10000000);
        if (orig.Session.DoNotLoad.Contains(entityID) || orig._IsInDoNotLoadIncreased(levelData, triggerData))
        {
            return;
        }

        switch ((!Level.LoadCustomEntity(triggerData, orig)) ? triggerData.Name : "")
        {
            case "eventTrigger":
                orig.Add(new EventTrigger(triggerData, levelOffset));
                break;
            case "musicFadeTrigger":
                orig.Add(new MusicFadeTrigger(triggerData, levelOffset));
                break;
            case "musicTrigger":
                orig.Add(new MusicTrigger(triggerData, levelOffset));
                break;
            case "altMusicTrigger":
                orig.Add(new AltMusicTrigger(triggerData, levelOffset));
                break;
            case "cameraOffsetTrigger":
                orig.Add(new CameraOffsetTrigger(triggerData, levelOffset));
                break;
            case "lightFadeTrigger":
                orig.Add(new LightFadeTrigger(triggerData, levelOffset));
                break;
            case "bloomFadeTrigger":
                orig.Add(new BloomFadeTrigger(triggerData, levelOffset));
                break;
            case "cameraTargetTrigger":
            {
                string text2 = triggerData.Attr("deleteFlag");
                if (string.IsNullOrEmpty(text2) || !orig.Session.GetFlag(text2))
                {
                    orig.Add(new CameraTargetTrigger(triggerData, levelOffset));
                }

                break;
            }
            case "cameraAdvanceTargetTrigger":
                orig.Add(new CameraAdvanceTargetTrigger(triggerData, levelOffset));
                break;
            case "respawnTargetTrigger":
                orig.Add(new RespawnTargetTrigger(triggerData, levelOffset));
                break;
            case "changeRespawnTrigger":
                orig.Add(new ChangeRespawnTrigger(triggerData, levelOffset));
                break;
            case "windTrigger":
                orig.Add(new WindTrigger(triggerData, levelOffset));
                break;
            case "windAttackTrigger":
                orig.Add(new WindAttackTrigger(triggerData, levelOffset));
                break;
            case "minitextboxTrigger":
                orig.Add(new MiniTextboxTrigger(triggerData, levelOffset, entityID));
                break;
            case "oshiroTrigger":
                orig.Add(new OshiroTrigger(triggerData, levelOffset));
                break;
            case "interactTrigger":
                orig.Add(new InteractTrigger(triggerData, levelOffset));
                break;
            case "checkpointBlockerTrigger":
                orig.Add(new CheckpointBlockerTrigger(triggerData, levelOffset));
                break;
            case "lookoutBlocker":
                orig.Add(new LookoutBlocker(triggerData, levelOffset));
                break;
            case "stopBoostTrigger":
                orig.Add(new StopBoostTrigger(triggerData, levelOffset));
                break;
            case "noRefillTrigger":
                orig.Add(new NoRefillTrigger(triggerData, levelOffset));
                break;
            case "ambienceParamTrigger":
                orig.Add(new AmbienceParamTrigger(triggerData, levelOffset));
                break;
            case "creditsTrigger":
                orig.Add(new CreditsTrigger(triggerData, levelOffset));
                break;
            case "goldenBerryCollectTrigger":
                orig.Add(new GoldBerryCollectTrigger(triggerData, levelOffset));
                break;
            case "moonGlitchBackgroundTrigger":
                orig.Add(new MoonGlitchBackgroundTrigger(triggerData, levelOffset));
                break;
            case "blackholeStrength":
                orig.Add(new BlackholeStrengthTrigger(triggerData, levelOffset));
                break;
            case "rumbleTrigger":
                orig.Add(new RumbleTrigger(triggerData, levelOffset, entityID));
                break;
            case "birdPathTrigger":
                orig.Add(new BirdPathTrigger(triggerData, levelOffset));
                break;
            case "spawnFacingTrigger":
                orig.Add(new SpawnFacingTrigger(triggerData, levelOffset));
                break;
            case "detachFollowersTrigger":
                orig.Add(new DetachStrawberryTrigger(triggerData, levelOffset));
                break;
        }
    }

    public static void AddEntityWithDecalData(this Level orig, DecalData fgDecalData, bool isFg)
    {
        LevelData levelData = orig.Session.LevelData;
        Vector2 levelOffset = new Vector2(levelData.Bounds.Left, levelData.Bounds.Top);

        int defaultDepth = isFg ? -10500 : 9000;
        Decal decal = new Decal(fgDecalData.Texture, levelOffset + fgDecalData.Position, fgDecalData.Scale, fgDecalData.Depth ?? defaultDepth, fgDecalData.Rotation,
            fgDecalData.ColorHex);
        if (fgDecalData.Depth.HasValue)
        {
            decal.DepthSetByPlacement = true;
        }

        if (fgDecalData.HasParallax())
        {
            decal.MakeParallax(fgDecalData.GetParallax());
            decal.ParallaxSetByPlacement = true;
        }

        orig.Add(decal);
    }
}