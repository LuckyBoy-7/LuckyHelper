using Celeste.Mod.Entities;
using LuckyHelper.Components;
using LuckyHelper.Extensions;
using LuckyHelper.Utils;
using MonoMod.Utils;

namespace LuckyHelper.Entities.Misc;

[CustomEntity("LuckyHelper/SpeedRedirect")]
public class SpeedRedirect : Entity
{
    // 吸引相关
    private int attractSpeed;
    private bool naiveMove;
    private int adjustX;
    private int adjustY;

    // 碰撞箱相关
    private ColliderType colliderType;
    private int radius;
    private int width;
    private int height;

    // 辅助显示相关
    private int spriteDepth;
    private string spriteXMLID;
    private Color borderColor;
    private Color innerColor;
    private float alpha;
    private bool showBorder;
    private bool showBackground;
    private bool showSprite;
    private bool flipSpriteX;
    private bool flipSpriteY;
    private float spriteRotation;

    // 触发相关
    private TriggerRedirectTiming triggerRedirectTiming;
    private SpeedRedirectDirType speedRedirectDirType;
    private bool preCollideWithPlayer;
    private bool collideWithPlayer;
    private float redirectDirX;
    private float redirectDirY;
    private float minCorrectionSpeed;
    private float maxCorrectionSpeed;
    private float maxShootSpeed;
    private float minShootSpeed;
    private float fixedShootSpeed;
    private float shootSpeedMultiplier = 1.2f;
    private SpeedRedirectStrengthType speedRedirectStrengthType;
    private Entity bg = new Entity();

    private bool shooting;


    private Vector2 playerSpeedOnEnter;

    private Vector2 ColliderCenter => Position + Collider.Center + new Vector2(adjustX, adjustY);


    public SpeedRedirect(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        Depth = -1;
        colliderType = data.Enum<ColliderType>("colliderType");

        Collider c;
        if (colliderType == ColliderType.Circle)
        {
            radius = data.Int("radius");
            c = new Circle(radius);
        }
        else
        {
            width = data.Int("boxWidth");
            height = data.Int("boxHeight");
            c = new Hitbox(width, height, -width / 2, -height / 2);
        }

        Collider = c;
        spriteDepth = data.Int("depth");
        attractSpeed = data.Int("attractSpeed", 80);
        naiveMove = data.Bool("naiveMove");
        adjustX = data.Int("adjustX");
        adjustY = data.Int("adjustY");
        spriteXMLID = data.Attr("spriteXMLID", "booster");
        showSprite = data.Bool("showSprite");
        flipSpriteX = data.Bool("flipSpriteX");
        flipSpriteY = data.Bool("flipSpriteY");
        spriteRotation = data.Float("spriteRotation");

        borderColor = data.HexColor("borderColor");
        innerColor = data.HexColor("innerColor");
        alpha = data.Float("alpha");
        showBorder = data.Bool("showBorder");
        showBackground = data.Bool("showBackground");

        triggerRedirectTiming = data.Enum<TriggerRedirectTiming>("triggerRedirectTiming");
        speedRedirectDirType = data.Enum<SpeedRedirectDirType>("speedRedirectDirType");
        speedRedirectStrengthType = data.Enum<SpeedRedirectStrengthType>("speedRedirectStrengthType");
        fixedShootSpeed = data.Float("fixedShootSpeed");
        minCorrectionSpeed = data.Float("minCorrectionSpeed");
        maxCorrectionSpeed = data.Float("maxCorrectionSpeed");
        maxShootSpeed = data.Float("maxShootSpeed");
        minShootSpeed = data.Float("minShootSpeed");
        if (minShootSpeed > maxShootSpeed)
            (minShootSpeed, maxShootSpeed) = (maxShootSpeed, minShootSpeed);

        shootSpeedMultiplier = data.Float("shootSpeedMultiplier");
        redirectDirX = data.Float("redirectDirX");
        redirectDirY = data.Float("redirectDirY");
        if (redirectDirX == 0 && redirectDirY == 0)
            redirectDirY = -1;
        else
        {
            float denominator = (float)Math.Sqrt(Math.Pow(redirectDirX, 2) + Math.Pow(redirectDirY, 2));
            redirectDirX /= denominator;
            redirectDirY /= denominator;
        }
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        bg.Visible = false;
        bg.Position = Position;
        bg.Depth = spriteDepth;

        RenderComponent renderComponent = new RenderComponent(true, true);
        bg.Add(renderComponent);
        // borderColor = new Color(35, 125, 255);
        // innerColor = new Color(150, 255, 255);
        renderComponent.OnRender += () =>
        {
            RenderUtils.RenderBoundsByColldierType(new RenderedBoundsByColliderTypeData()
            {
                ColliderType = colliderType,
                Position = bg.Position,
                Radius = radius,
                Width = width,
                Height = height,
                InnerColor = innerColor,
                BorderColor = borderColor,
                Alpha = alpha,
                ShowBackground = showBackground,
                ShowBorder = showBorder
            });
        };
        if (showSprite)
        {
            Sprite sprite = GFX.SpriteBank.Create(spriteXMLID);
            bg.Add(sprite);
            sprite.FlipX = flipSpriteX;
            sprite.FlipY = flipSpriteY;
            sprite.Rotation = Calc.DegToRad * spriteRotation;
        }

        Scene.Add(bg);
    }

    public override void Render()
    {
        base.Render();
        bg.Position = Position;
        bg.Render();
    }


    public override void Update()
    {
        base.Update();

        Player player = this.GetEntity<Player>();
        preCollideWithPlayer = collideWithPlayer;
        collideWithPlayer = player?.CollideCheck(this) ?? false;

        bool onEnter = !preCollideWithPlayer && collideWithPlayer;
        bool onStay = collideWithPlayer;
        bool onLeave = preCollideWithPlayer && !collideWithPlayer;

        // 此时 player 一定不为 null
        if (onEnter)
        {
            playerSpeedOnEnter = player.Speed;

            // 如果是冲进来的就设成 normal
            if (player.StateMachine.State == Player.StDash)
            {
                player.StateMachine.State = Player.StNormal;
            }


            if (triggerRedirectTiming == TriggerRedirectTiming.OnEnter)
            {
                TryShootPlayer(player);
            }
        }

        if (onStay && triggerRedirectTiming == TriggerRedirectTiming.OnAttractedToCenter)
        {
            if (!shooting)
            {
                if (player.StateMachine.State == Player.StDash)
                    player.StateMachine.State = Player.StNormal;

                var config = new PlayerPinnedHandler(player, naiveMove);
                var targetPos = Calc.Approach(config.Position, ColliderCenter, attractSpeed * Engine.DeltaTime);
                config.Position = targetPos;
                var dyn = new DynamicData(player);
                config.PinnedAction(dyn);

                bool nearCenter = (ColliderCenter - targetPos).Length() < 5;
                if (nearCenter)
                {
                    TryShootPlayer(player);
                }
            }
        }

        if (onLeave)
        {
            shooting = false;
        }
    }

    private void TryShootPlayer(Player player)
    {
        if (shooting)
            return;
        shooting = true;

        Vector2 speed = playerSpeedOnEnter;
        float speedAmount = 0;
        switch (speedRedirectStrengthType)
        {
            case SpeedRedirectStrengthType.PlayerEnterSpeed:
                speedAmount = speed.Length() * shootSpeedMultiplier;
                break;
            case SpeedRedirectStrengthType.Fixed:
                speedAmount = fixedShootSpeed;
                break;
            case SpeedRedirectStrengthType.Correction:
                float curAmount = speed.Length();
                if (minCorrectionSpeed <= curAmount && curAmount <= maxCorrectionSpeed)
                {
                    speedAmount = fixedShootSpeed;
                }
                else
                {
                    speedAmount = speed.Length() * shootSpeedMultiplier;
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        // 限制速度范围
        speedAmount = Math.Clamp(speedAmount, minShootSpeed, maxShootSpeed);

        Vector2 speedDir = speed;
        speedDir.Normalize();

        speed = speedDir * speedAmount;

        if (speedRedirectDirType == SpeedRedirectDirType.Rebound)
        {
            player.Speed = speed * -1;
        }
        else if (speedRedirectDirType == SpeedRedirectDirType.flipX)
        {
            player.Speed = speed.WithX(speed.X * -1);
        }
        else if (speedRedirectDirType == SpeedRedirectDirType.flipY)
        {
            player.Speed = speed.WithY(speed.Y * -1);
        }
        else if (speedRedirectDirType == SpeedRedirectDirType.Custom)
        {
            Vector2 velocity = new Vector2(redirectDirX, redirectDirY) * speedAmount;
            player.Speed = velocity;
            // LogUtils.LogWarning($"{player.Speed}");
        }
    }
}