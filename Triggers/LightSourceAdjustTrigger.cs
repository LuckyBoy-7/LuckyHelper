using Celeste.Mod.Entities;
using LuckyHelper.Module;
using BloomRenderer = On.Celeste.BloomRenderer;
using LightingRenderer = On.Celeste.LightingRenderer;

namespace LuckyHelper.Triggers;

[CustomEntity("LuckyHelper/LightSourceAdjustTrigger")]
[Tracked]
public class LightSourceAdjustTrigger : Trigger
{
    public static float Factor
    {
        get => LuckyHelperModule.Session.LightFactor;
        set => LuckyHelperModule.Session.LightFactor = value;
    }

    private bool affectRadius;
    private bool affectAlpha;
    private string targets = "Celeste.Player";
    private float offsetFrom = 0;
    private float offsetTo = 2;


    private PositionModes positionMode;

    public LightSourceAdjustTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        targets = data.Attr("targets");
        offsetFrom = data.Float("offsetFrom");
        offsetTo = data.Float("offsetTo");
        positionMode = data.Enum<PositionModes>("positionMode");
        affectRadius = data.Bool("affectRadius");
        affectAlpha = data.Bool("affectAlpha");
    }

    [Load]
    public static void Load()
    {
        On.Celeste.BloomRenderer.Apply += BloomRendererOnApply;
        On.Celeste.LightingRenderer.BeforeRender += LightingRendererOnBeforeRender;
    }


    [Unload]
    public static void Unload()
    {
        On.Celeste.BloomRenderer.Apply -= BloomRendererOnApply;
        On.Celeste.LightingRenderer.BeforeRender -= LightingRendererOnBeforeRender;
    }


    private static void BloomRendererOnApply(BloomRenderer.orig_Apply orig, Celeste.BloomRenderer self, VirtualRenderTarget target, Scene scene)
    {
        
        if (!LuckyHelperModule.Session.LightFactorOn && scene.Tracker.GetEntity<LightSourceAdjustTrigger>() != null)
            LuckyHelperModule.Session.LightFactorOn = true;
        if (!LuckyHelperModule.Session.LightFactorOn)
        {
            orig(self, target, scene);
            return;
        }


        List<Tuple<BloomPoint, float, float>> backup = new List<Tuple<BloomPoint, float, float>>();

        // if (count++ == 0)
        foreach (BloomPoint bloomPoint in scene.Tracker.GetComponents<BloomPoint>())
        {
            Entity entity = bloomPoint.Entity;
            if (EntityInTargets(entity))
            {
                backup.Add(new(bloomPoint, bloomPoint.Alpha, bloomPoint.Radius));
                if (LuckyHelperModule.Session.AffectLightAlpha)
                    bloomPoint.Alpha *= Factor;
                if (LuckyHelperModule.Session.AffectLightRadius)
                {
                    bloomPoint.Radius *= Factor;
                }
            }
        }

        orig(self, target, scene);

        foreach (var (bloomPoint, alpha, radius) in backup)
        {
            bloomPoint.Alpha = alpha;
            bloomPoint.Radius = radius;
        }
    }


    private static void LightingRendererOnBeforeRender(LightingRenderer.orig_BeforeRender orig, Celeste.LightingRenderer self, Scene scene)
    {
        if (!LuckyHelperModule.Session.LightFactorOn && scene.Tracker.GetEntity<LightSourceAdjustTrigger>() != null)
            LuckyHelperModule.Session.LightFactorOn = true;
        if (!LuckyHelperModule.Session.LightFactorOn)
        {
            orig(self, scene);
            return;
        }


        List<Tuple<VertexLight, float, float, float>> backup = new List<Tuple<VertexLight, float, float, float>>();

        // if (count++ == 0)
        foreach (VertexLight vertexLight in scene.Tracker.GetComponents<VertexLight>())
        {
            Entity entity = vertexLight.Entity;
            if (EntityInTargets(entity))
            {
                backup.Add(new(vertexLight, vertexLight.Alpha, vertexLight.StartRadius, vertexLight.EndRadius));
                if (LuckyHelperModule.Session.AffectLightAlpha)
                    vertexLight.Alpha *= Factor;
                if (LuckyHelperModule.Session.AffectLightRadius)
                {
                    vertexLight.StartRadius *= Factor;
                    vertexLight.EndRadius *= Factor;
                }
            }
        }

        orig(self, scene);

        foreach (var (vertexLight, alpha, startRadius, endRadius) in backup)
        {
            vertexLight.Alpha = alpha;
            vertexLight.StartRadius = startRadius;
            vertexLight.EndRadius = endRadius;
        }
    }

    public override void OnStay(Player player)
    {
        base.OnStay(player);
        Factor = MathHelper.Lerp(offsetFrom, offsetTo, GetPositionLerp(player, positionMode));
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);

        // register
        LuckyHelperModule.Session.LightFactorTargets.Clear();
        foreach (var str in targets.Split(","))
        {
            LuckyHelperModule.Session.LightFactorTargets.Add(str.Trim());
        }

        LuckyHelperModule.Session.AffectLightRadius = affectRadius;
        LuckyHelperModule.Session.AffectLightAlpha = affectAlpha;
    }

    private static bool EntityInTargets(Entity entity) => LuckyHelperModule.Session.LightFactorTargets.Contains(entity.GetType().ToString());
}