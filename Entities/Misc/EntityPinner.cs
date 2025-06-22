using Celeste.Mod.Entities;
using LuckyHelper.Components;
using LuckyHelper.Extensions;
using LuckyHelper.Modules;
using LuckyHelper.Utils;
using MonoMod.Utils;

namespace LuckyHelper.Entities;

[CustomEntity("LuckyHelper/EntityPinner")]
public class EntityPinner : Entity
{
    private HashSet<string> briefTypes = new();
    private int spriteDepth;
    private int attractSpeed;
    private bool naiveMove;
    private int adjustX;
    private int adjustY;
    private string spriteXMLID;
    private ColliderTypes colliderType;
    private int radius;
    private int width;
    private int height;
    private Color borderColor;
    private Color innerColor;
    private float alpha;
    private bool showBorder;
    private bool showBackground;


    public EntityPinner(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        Depth = -1;
        colliderType = data.Enum<ColliderTypes>("colliderType");

        Collider c;
        if (colliderType == ColliderTypes.Circle)
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

        borderColor = data.HexColor("borderColor");
        innerColor = data.HexColor("innerColor");
        alpha = data.Float("alpha");
        showBorder = data.Bool("showBorder");
        showBackground = data.Bool("showBackground");


        briefTypes = ParseUtils.ParseTypesStringToBriefNames(data.Attr("types"));
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        Entity bg = new Entity();
        bg.Position = Position;
        bg.Depth = spriteDepth;

        RenderComponent renderComponent = new RenderComponent(true, true);
        bg.Add(renderComponent);
        // borderColor = new Color(35, 125, 255);
        // innerColor = new Color(150, 255, 255);
        renderComponent.OnRender += () =>
        {
            if (colliderType == ColliderTypes.Circle)
            {
                Vector2 start = Position - new Vector2(radius, radius);
                if (showBackground)
                    Draw.Rect(start, radius * 2, radius * 2, innerColor * alpha);
                if (showBorder)
                    Draw.HollowRect(start, radius * 2, radius * 2, borderColor * alpha);
            }
            else if (colliderType == ColliderTypes.Rectangle)
            {
                Vector2 start = Position - new Vector2(width / 2, height / 2);
                if (showBackground)
                    Draw.Rect(start, width, height, innerColor * alpha);
                if (showBorder)
                    Draw.HollowRect(start, width, height, borderColor * alpha);
            }
        };
        bg.Add(GFX.SpriteBank.Create(spriteXMLID));
        Scene.Add(bg);
    }


    public override void Update()
    {
        base.Update();

        foreach (var type in briefTypes)
        {
            foreach (var entity in TypeToObjectsModule.BriefTypeToEntities[type])
            {
                if (entity.CollideCheck(this))
                {
                    var config = GetHandler(entity);
                    if (config == null)
                        continue;

                    var targetPos = Calc.Approach(config.Position, Position + Collider.Center + new Vector2(adjustX, adjustY), attractSpeed * Engine.DeltaTime);
                    config.Position = targetPos;
                    var dyn = new DynamicData(entity);
                    config.PinnedAction(dyn);
                }
            }
        }
    }

    private PinnedEntityHandler GetHandler(Entity entity)
    {
        PinnedEntityHandler config;
        Holdable holdable;
        if (entity is Player player)
        {
            if (player.StateMachine.State == Player.StDash)
                return null;
            config = new PlayerPinnedHandler(entity, naiveMove);
        }
        else if (entity is Seeker)
            config = new SeekerPinnedHandler(entity, naiveMove);
        else if ((holdable = entity.Get<Holdable>()) != null)
        {
            if (holdable.Holder != null)
                return null;
            config = new HoldablePinnedHandler(entity, naiveMove);
        }
        else if (entity is Puffer)
            config = new PufferPinnedHandler(entity, naiveMove);
        else if (entity is FireBall)
            config = new FireBallPinnedHandler(entity, naiveMove);
        else if (entity is Bumper)
            config = new BumperPinnedHandler(entity, naiveMove);
        else
            config = new DummyPinnedHandler(entity, naiveMove);

        return config;
    }
}

public abstract class PinnedEntityHandler(Entity entity, bool naiveMove)
{
    protected Entity entity = entity;
    protected bool naiveMove = naiveMove;

    public abstract void PinnedAction(DynamicData dyn);

    public virtual Vector2 Position
    {
        get
        {
            if (entity.Collider != null)
                return entity.Position + entity.Collider.Center;
            return entity.Position;
        }
        set
        {
            Vector2 targetPos = entity.Collider != null ? value - entity.Collider.Center : value;

            if (!naiveMove && entity is Actor actor)
            {
                actor.MoveToX(targetPos.X);
                actor.MoveToY(targetPos.Y);
            }
            else if (!naiveMove && entity is Solid solid)
            {
                solid.MoveToX(targetPos.X);
                solid.MoveToY(targetPos.Y);
            }
            else
            {
                entity.Position = targetPos;
            }
        }
    }
}

public class DummyPinnedHandler(Entity entity, bool naiveMove) : PinnedEntityHandler(entity, naiveMove)
{
    public override void PinnedAction(DynamicData dyn)
    {
    }
}

public class PlayerPinnedHandler(Entity entity, bool naiveMove) : PinnedEntityHandler(entity, naiveMove)
{
    private Player Player => (Player)entity;


    public override void PinnedAction(DynamicData dyn)
    {
        dyn.Set("Speed", Vector2.Zero);
    }
}

public class HoldablePinnedHandler(Entity entity, bool naiveMove) : PinnedEntityHandler(entity, naiveMove)
{
    public override void PinnedAction(DynamicData dyn)
    {
        dyn.Set("Speed", Vector2.Zero);
    }
}

public class SeekerPinnedHandler(Entity entity, bool naiveMove) : PinnedEntityHandler(entity, naiveMove)
{
    public override void PinnedAction(DynamicData dyn)
    {
        dyn.Set("Speed", Vector2.Zero);
    }
}

public class PufferPinnedHandler(Entity entity, bool naiveMove) : PinnedEntityHandler(entity, naiveMove)
{
    private Puffer Puffer => (Puffer)entity;

    public override Vector2 Position
    {
        get => naiveMove ? Puffer.Position : Puffer.anchorPosition;
        set
        {
            if (naiveMove)
            {
                Puffer.Position = value;
                Puffer.lastSpeedPosition = value;
            }
            else
            {
                Puffer.anchorPosition = value;
                Puffer.lastSpeedPosition = Puffer.Position - Vector2.UnitY;
            }
        }
    }

    public override void PinnedAction(DynamicData dyn)
    {
        dyn.Set("hitSpeed", Vector2.Zero);
    }
}

public class FireBallPinnedHandler(Entity entity, bool naiveMove) : PinnedEntityHandler(entity, naiveMove)
{
    public override void PinnedAction(DynamicData dyn)
    {
        FireBall fireBall = entity as FireBall;
        dyn.Set("speed", 0f);
        fireBall.nodes = [entity.Position];
        fireBall.index = 0;
        fireBall.percent = -1;
    }
}

public class BumperPinnedHandler(Entity entity, bool naiveMove) : PinnedEntityHandler(entity, naiveMove)
{
    private Bumper Bumper => (Bumper)entity;

    public override Vector2 Position
    {
        get => Bumper.anchor;
        set => Bumper.anchor = value;
    }

    public override void PinnedAction(DynamicData dyn)
    {
        Bumper bumper = entity as Bumper;
        bumper.Get<Tween>()?.RemoveSelf();
    }
}