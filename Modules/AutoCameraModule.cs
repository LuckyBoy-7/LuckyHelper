// using Lucky.Kits.Collections;
// using LuckyHelper.Extensions;
// using LuckyHelper.Module;
// using LuckyHelper.Utils;
// using Microsoft.Xna.Framework.Input;
// using Player = Celeste.Player;
//
// namespace LuckyHelper.Modules;
//
// public class AutoCameraModule
// {
//     public static bool on = true;
//     private static CameraFader faderX = new CameraFader(20f);
//     private static CameraFader faderY = new CameraFader(20f);
//
//     public static bool Debugging => DebugModule.debug;
//
//     [Load]
//     public static void Load()
//     {
//         float testValue = 0.04f;
//         faderX.Add(0, 110, 0, 20, testValue);
//         faderX.Add(110, 170, 20, 40, testValue);
//         faderX.Add(170, 240, 40, 60, testValue);
//         faderX.Add(240, 325, 60, 80, testValue);
//         faderX.Add(325, 390, 80, 110, testValue);
//         faderX.Add(390, 500, 110, 120, testValue);
//         faderX.Add(500, 3000, 120, 140, testValue);
//
//         faderY.Add(0, 90, 0, 20, testValue);
//         faderY.Add(90, 160, 20, 40, testValue);
//         faderY.Add(16, 240, 40, 60, testValue);
//         faderY.Add(240, 1000, 60, 80, testValue);
//
//         On.Celeste.Player.Update += PlayerOnUpdate;
//         On.Celeste.Player.Added += PlayerOnAdded;
//         On.Celeste.Player.Render += PlayerOnRender;
//     }
//
//     private static void PlayerOnRender(On.Celeste.Player.orig_Render orig, Player self)
//     {
//         if (Debugging)
//         {
//             Vector2 origPos = self.level.Camera.Position + new Vector2(160, 90);
//             Vector2 topLeft = origPos + new Vector2(-faderX.ComfortZoneSize, -faderY.ComfortZoneSize);
//             Draw.Rect(topLeft, faderX.ComfortZoneSize * 2, faderY.ComfortZoneSize * 2, Color.Green);
//         }
//
//         orig(self);
//     }
//
//
//     [Unload]
//     public static void Unload()
//     {
//         On.Celeste.Player.Update -= PlayerOnUpdate;
//         On.Celeste.Player.Added -= PlayerOnAdded;
//         On.Celeste.Player.Render -= PlayerOnRender;
//     }
//
//     private static void PlayerOnAdded(On.Celeste.Player.orig_Added orig, Player self, Scene scene)
//     {
//         orig(self, scene);
//
//         faderX.Offset = new(() => self.level.CameraOffset.X, value => self.level.CameraOffset.X = value);
//         faderX.CameraPos = new(() => self.level.Camera.Position.X, value => self.level.Camera.Position = self.level.Camera.Position.WithX(value));
//         faderX.CameraTarget = new(() => self.CameraTarget.X, null);
//         faderX.PlayerPos = new(() => self.Position.X, null);
//
//         faderY.Offset = new(() => self.level.CameraOffset.Y, value => self.level.CameraOffset.Y = value);
//         faderY.CameraPos = new(() => self.level.Camera.Position.Y, value => self.level.Camera.Position = self.level.Camera.Position.WithY(value));
//         faderY.CameraTarget = new(() => self.CameraTarget.Y, null);
//         faderY.PlayerPos = new(() => self.Position.Y, null);
//
//         faderX.ResetValue();
//         faderY.ResetValue();
//     }
//
//     private static void PlayerOnUpdate(On.Celeste.Player.orig_Update orig, Player self)
//     {
//         if (MInput.Keyboard.Pressed(Keys.Y))
//         {
//             on = !on;
//             LogUtils.LogInfo("on: " + on);
//         }
//
//         if (!on)
//         {
//             orig(self);
//             return;
//         }
//
//         Vector2 origPlayerPosition = self.ExactPosition;
//         Vector2 origCameraPos = self.level.Camera.Position;
//
//         // 抵消原来lerp的效果, 后续自己控制lerp
//         orig(self);
//         // self.level.Camera.Position = origCameraPos;
//         self.level.Camera.Position = self.level.Camera.Position.WithX(origCameraPos.X);
//
//         // self.level.Camera.Position = origCameraPos + (self.CameraTarget - origCameraPos * ));
//         Vector2 offsetPosition = self.ExactPosition - origPlayerPosition;
//         Vector2 cameraCenter = self.level.Camera.Position + new Vector2(160, 90);
//         Vector2 offsetToCameraCenter = self.ExactPosition - cameraCenter;
//
//         // set
//         faderX.Update(offsetToCameraCenter.X, (int)(offsetPosition.X * 60));
//         // faderY.Update(offsetToCameraCenter.Y, (int)(offsetPosition.Y * 60));
//
//         // if(self.Position.X < cameraCenter.X - faderX.ComfortZoneSize
//         //    || self.Position.X > cameraCenter.X + faderX.ComfortZoneSize
//         //    ||self.Position.Y < cameraCenter.Y - faderY.ComfortZoneSize
//         //    || self.Position.Y > cameraCenter.Y + faderY.ComfortZoneSize)
//
//         // Vector2 targetPos = Vector2.Lerp(origCameraPos, self.CameraTarget, 1f - (float)Math.Pow(0.01f, Engine.DeltaTime));
//         // if (faderX.SameDirAndStartOutOfComfortZone)
//         //     self.level.Camera.Position = self.level.Camera.Position.WithX(targetPos.X);
//         // if (faderY.SameDirAndStartOutOfComfortZone)
//         //     self.level.Camera.Position = self.level.Camera.Position.WithY(targetPos.Y);
//
//         // debug
//         // LogUtils.LogInfo((offsetPosition.X * 60).ToString());
//         // LogUtils.LogInfo($"{self.level.CameraOffset.X} {self.level.CameraOffset.Y}");
//         // LogUtils.LogInfo($"{self.Position} {offsetToCameraCenter}");
//     }
//
//
//     class CameraFader
//     {
//         private List<Tuple<float, float, float, float, float>> data = new();
//         private float decrease;
//         private float lerpValue;
//         public bool SameDirAndStartOutOfComfortZone;
//         public bool InComfortZone;
//         private int startLerpSpeedDir;
//
//         public DelegateValue<float> Offset; // cameraoffset的某一个维度
//         public DelegateValue<float> CameraPos; // camera.Position的某一个维度
//         public DelegateValue<float> CameraTarget; // self.CameraTarget的某一个维度
//         public DelegateValue<float> PlayerPos; // self.CameraTarget的某一个维度
//
//         private float prePlayerSpeed;
//         private float curPlayerSpeed;
//         private Deque<float> preCameraLerpDeltas = new Deque<float>();
//         private const int PreCameraLerpDeltasNumber = 60;
//         private float playerPosToCameraPosDelta = float.MaxValue;
//
//         public CameraFader(float comfortZoneSize = 30f, float decrease = 2f)
//         {
//             this.ComfortZoneSize = comfortZoneSize;
//             this.decrease = decrease;
//         }
//
//         public float ComfortZoneSize { get; } // 舒适区大小(只算一边)
//
//         // 简单来说我们认为速率(位移)跟值是有映射关系的, 比如minVelocity=0, maxVelocity=90, minValue=0.5, 可以看作不动时, 相机在原点, player向右走路时, 相机offset在偏有的地方
//         public void Add(float minVelocity, float maxVelocity, float minValue, float maxValue, float increase = 1)
//         {
//             data.Add(new(minVelocity, maxVelocity, minValue, maxValue, increase));
//         }
//
//         private void UpdateOffset(float offsetToCameraCenter, float curSpeed)
//         {
//             prePlayerSpeed = curPlayerSpeed;
//             curPlayerSpeed = curSpeed;
//             
//             if (curSpeed == 0)
//             {
//                 Offset.Value = Calc.Approach(Offset.Value, 0, decrease);
//                 SameDirAndStartOutOfComfortZone = false;
//                 // value = MathHelper.Lerp(value, 0, decreaseLerpK);
//                 // startLerpSpeedDir = 0;
//                 return;
//             }
//
//             offsetToCameraCenter = Math.Abs(offsetToCameraCenter);
//             if (offsetToCameraCenter <= ComfortZoneSize && !SameDirAndStartOutOfComfortZone)
//                 return;
//
//
//             if (startLerpSpeedDir == Single.Sign(curSpeed))
//                 SameDirAndStartOutOfComfortZone = true;
//             else
//             {
//                 startLerpSpeedDir = Single.Sign(curSpeed);
//                 SameDirAndStartOutOfComfortZone = false;
//             }
//
//             // LogUtils.LogWarning($"{startLerpSpeedDir} {curSpeed} {sameDirAndStartOutOfComfortZone}");
//             // LogUtils.LogInfo("StartLerpSpeedDir: " + startLerpSpeedDir + "Single.Sign(curSpeed)" + Single.Sign(curSpeed));
//             // 标量
//             float curVelocity = Math.Abs(curSpeed);
//             foreach (var (minVelocity, maxVelocity, minValue, maxValue, increase) in data)
//             {
//                 // 只要minVelocity < curVelocity <= maxVelocity的
//                 if (curVelocity <= minVelocity || curVelocity > maxVelocity)
//                     continue;
//
//                 if (curSpeed > 0)
//                 {
//                     if (Offset.Value < maxValue)
//                     {
//                         Offset.Value += curSpeed * increase;
//                         if (Offset.Value > maxValue)
//                             Offset.Value = maxValue;
//                     }
//                 }
//                 else
//                 {
//                     if (Offset.Value > -maxValue)
//                     {
//                         Offset.Value += curSpeed * increase;
//                         if (Offset.Value < -maxValue)
//                             Offset.Value = -maxValue;
//                     }
//                 }
//
//
//                 return;
//             }
//
//             // value = MathHelper.Lerp(value, 0, decreaseLerpK);
//             Offset.Value = Calc.Approach(Offset.Value, 0, decrease);
//         }
//
//         public bool CameraCantLerpMore()
//         {
//             if (preCameraLerpDeltas.Count < PreCameraLerpDeltasNumber)
//                 return false;
//             int closeNumberCount = 0;
//             for (var i = 0; i < PreCameraLerpDeltasNumber - 1; i++)
//             {
//                 if (preCameraLerpDeltas[i + 1] - preCameraLerpDeltas[i] <= 0.1f)
//                     closeNumberCount += 1;
//             }
//
//             return closeNumberCount > PreCameraLerpDeltasNumber / 3 * 2;
//         }
//
//         public void Update(float offsetToCameraCenter, float curSpeed)
//         {
//             UpdateOffset(offsetToCameraCenter, curSpeed);
//             float targetPos = MathHelper.Lerp(CameraPos.Value, CameraTarget.Value, 1f - (float)Math.Pow(0.01f, Engine.DeltaTime));
//
//             LogUtils.LogInfo(
//                 $"SameDirAndStartOutOfComfortZone: {SameDirAndStartOutOfComfortZone}, curPlayerSpeed - prePlayerSpeed: {curPlayerSpeed - prePlayerSpeed}, CameraCantLerpMore: {CameraCantLerpMore()}");
//
//
//             if (curSpeed == 0)
//             {
//                 LogUtils.LogInfo($"sadfasdf");
//                 CameraPos.Value = targetPos;
//                 return;
//             }
//
//             if (!SameDirAndStartOutOfComfortZone)
//             {
//                 if (preCameraLerpDeltas.Count > 0)
//                     preCameraLerpDeltas.PopLeft();
//                 return;
//             }
//
//             float cameraLerpDelta = targetPos - CameraPos.Value;
//             LogUtils.LogInfo($"cameraLerpDelta: {cameraLerpDelta}, CameraTarget: {CameraTarget.Value}, CameraPos: {CameraPos.Value}");
//             preCameraLerpDeltas.Append(cameraLerpDelta);
//             if (preCameraLerpDeltas.Count > PreCameraLerpDeltasNumber)
//                 preCameraLerpDeltas.PopLeft();
//
//             if (Math.Abs(curPlayerSpeed - prePlayerSpeed) < 0.1f && curPlayerSpeed <= 130 && CameraCantLerpMore())
//             {
//                 float curPlayerPosToCameraPosDelta = (int)CameraPos.Value - PlayerPos.Value;
//                 if (Math.Abs(curPlayerPosToCameraPosDelta - playerPosToCameraPosDelta) > 2f)
//                 {
//                     playerPosToCameraPosDelta = curPlayerPosToCameraPosDelta;
//                 }
//
//                 // LogUtils.LogInfo("true");
//                 // CameraPos.Value = targetPos;
//                 CameraPos.Value = PlayerPos.Value + playerPosToCameraPosDelta;
//                 // CameraPos.Value = targetPos;
//             }
//             else
//             {
//                 // LogUtils.LogInfo(1.ToString());
//                 CameraPos.Value = targetPos;
//             }
//         }
//
//         public void ResetValue()
//         {
//             Offset.Value = 0;
//         }
//     }
// }