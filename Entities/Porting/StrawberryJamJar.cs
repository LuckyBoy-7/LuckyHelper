using Celeste.Mod.CollabUtils2.Triggers;
using Celeste.Mod.Entities;
using LuckyHelper.Module;
using LuckyHelper.Modules;
using LuckyHelper.Utils;
using Microsoft.Xna.Framework.Input;

namespace LuckyHelper.Entities.Porting;

// ported from strawberry jam, someone needs it :)
[CustomEntity("LuckyHelper/StrawberryJamJar")]
public class StrawberryJamJar : Entity
{
    private readonly string map;
    private readonly string returnToLobbyMode;
    private readonly bool allowSaving;
    private readonly bool debugMode;
    private readonly Sprite sprite;

    public StrawberryJamJar(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        map = data.Attr("map");
        returnToLobbyMode = data.Attr("returnToLobbyMode");
        allowSaving = data.Bool("allowSaving");
        debugMode = data.Bool("debugMode");
        string spriteId = data.Attr("spriteId");
        string soundPath = data.Attr("soundPath");

        Depth = 1000;

        AreaData areaData = AreaData.Get(map);
        bool mapHasCompleted = false;
        if (areaData != null)
        {
            AreaStats areaStatsFor = SaveData.Instance.GetAreaStatsFor(areaData.ToKey());
            mapHasCompleted = areaStatsFor != null && areaStatsFor.Modes[0].Completed;
        }

        string animationId;
        if (mapHasCompleted)
        {
            if (!LuckyHelperModule.SaveData.FilledJamJarSIDs.Contains(map))
            {
                animationId = "before_fill";
                LuckyHelperModule.SaveData.FilledJamJarSIDs.Add(map);
            }
            else
            {
                animationId = "full";
            }
        }
        else
        {
            animationId = "empty";
        }

        if (!GFX.SpriteBank.Has(spriteId))
        {
            LogUtils.LogWarning($"SpriteId of jamJar doesn't exist: '{spriteId}'");
            return;
        }

        sprite = GFX.SpriteBank.Create(spriteId);
        sprite.Play(animationId);
        Add(sprite);
        sprite.OnChange = (previousAnimationId, currentAnimationId) =>
        {
            if (previousAnimationId == "before_fill" && currentAnimationId == "fill")
            {
                SoundSource soundSource = new SoundSource(new Vector2(0f, -20f), soundPath)
                {
                    RemoveOnOneshotEnd = true
                };
                soundSource.instance.setVolume(0.3f);
                Add(soundSource);
            }
        };
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        if (ModCompatModule.CollbaUtils2Loaded)
            AddChapterPanelTrigger(scene);
        else
            RemoveSelf();
    }

    private void AddChapterPanelTrigger(Scene scene)
    {
        scene.Add(new ChapterPanelTrigger(new EntityData
        {
            Position = Position - new Vector2(24f, 32f),
            Width = 48,
            Height = 32,
            Nodes = new[] { Position - new Vector2(0f, 32f) },
            Values = new Dictionary<string, object>
            {
                { "map", map },
                { "returnToLobbyMode", returnToLobbyMode },
                { "allowSaving", allowSaving }
            }
        }, Vector2.Zero));
    }

    public override void Update()
    {
        base.Update();

        if (!debugMode)
            return;

        if (MInput.Keyboard.Pressed(Keys.Enter))
        {
            sprite.Play("before_fill");
        }
    }
}