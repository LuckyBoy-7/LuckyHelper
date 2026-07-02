using Celeste.Mod.Entities;
using LuckyHelper.Module;
using LuckyHelper.Utils;
using BloomRenderer = On.Celeste.BloomRenderer;
using LightingRenderer = On.Celeste.LightingRenderer;

namespace LuckyHelper.Triggers;

[CustomEntity("LuckyHelper/LightSourceAdjustTrigger")]
[Tracked]
public class LightSourceAdjustTrigger : PositionTrigger
{
    private bool affectRadius;
    private bool affectAlpha;
    private HashSet<string> targets = new();

    public LightSourceAdjustTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        targets = ParseUtils.ParseTypesStringToBriefNames(data.Attr("targets"));

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
        List<Tuple<BloomPoint, float, float>> backup = new List<Tuple<BloomPoint, float, float>>();
        var targetToAlpha = LuckyHelperModule.Session.LightTargetToAlpha;
        var targetToRadius = LuckyHelperModule.Session.LightTargetToRadius;

        foreach (BloomPoint bloomPoint in scene.Tracker.GetComponents<BloomPoint>())
        {
            Entity entity = bloomPoint.Entity;
            string entityTypeName = entity.GetType().Name;


            float alpha = 1;
            float radius = 1;
            if (targetToAlpha.TryGetValue(entityTypeName, out var a))
                alpha = a;
            if (targetToRadius.TryGetValue(entityTypeName, out var r))
                radius = r;
            if (alpha != 1 || radius != 1)
            {
                backup.Add(new(bloomPoint, bloomPoint.Alpha, bloomPoint.Radius));
                bloomPoint.Alpha *= alpha;
                bloomPoint.Radius *= radius;
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
        List<Tuple<VertexLight, float, float, float>> backup = new List<Tuple<VertexLight, float, float, float>>();
        var targetToAlpha = LuckyHelperModule.Session.LightTargetToAlpha;
        var targetToRadius = LuckyHelperModule.Session.LightTargetToRadius;
        foreach (VertexLight vertexLight in scene.Tracker.GetComponents<VertexLight>())
        {
            Entity entity = vertexLight.Entity;
            string entityTypeName = entity.GetType().Name;


            float alpha = 1;
            float radius = 1;
            if (targetToAlpha.TryGetValue(entityTypeName, out var a))
                alpha = a;
            if (targetToRadius.TryGetValue(entityTypeName, out var r))
                radius = r;
            if (alpha != 1 || radius != 1)
            {
                backup.Add(new(vertexLight, vertexLight.Alpha, vertexLight.StartRadius, vertexLight.EndRadius));
                vertexLight.Alpha *= alpha;
                vertexLight.StartRadius *= radius;
                vertexLight.EndRadius *= radius;
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

    public override void OnSetValue(float factor)
    {
        var session = LuckyHelperModule.Session;
        foreach (string target in targets)
        {
            if (affectAlpha)
                session.LightTargetToAlpha[target] = factor;
            if (affectRadius)
                session.LightTargetToRadius[target] = factor;
        }
    }
}