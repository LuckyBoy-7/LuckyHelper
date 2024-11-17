using Celeste.Mod.Entities;
using ExtendedVariants.Module;
using ExtendedVariants.Variants;
using LuckyHelper.Extensions;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Utils;

namespace LuckyHelper.Entities;

[CustomEntity("LuckyHelper/CustomWater")]
[TrackedAs(typeof(Water))]
[Tracked]
public class CustomWater : Water
{
    private Color color;
    private Color preSurfaceColor;
    private Color preFillColor;
    public bool DisableSurfaceJump;
    public bool DisableSwimRise = true;
    public float MaxSpeedMultiplierX;
    public float MaxSpeedMultiplierY;
    public float AccelerationMultiplierX;
    public float AccelerationMultiplierY;

    public bool KillPlayer = false;
    public float KillPlayerDelay = 0;
    private float killPlayerTimer = 0;

    public bool PlayerLoseControl = true;
    public float PlayerGravity;
    public bool PlayerCanJump;
    public bool RefillExtraJump;
    
    // todo: gravity

    public CustomWater(EntityData data, Vector2 offset) : base(data, offset)
    {
        // disableSurfaceJump
        DisableSurfaceJump = data.Bool("disableSurfaceJump");
        DisableSwimRise = data.Bool("disableSwimRise");
        MaxSpeedMultiplierX = data.Float("maxSpeedMultiplierX");
        MaxSpeedMultiplierY = data.Float("maxSpeedMultiplierY");
        AccelerationMultiplierX = data.Float("accelerationMultiplierX");
        AccelerationMultiplierY = data.Float("accelerationMultiplierY");
        KillPlayer = data.Bool("killPlayer");
        KillPlayerDelay = data.Float("killPlayerDelay");
        PlayerLoseControl = data.Bool("playerLoseControl");
        PlayerGravity = data.Float("playerGravity");
        PlayerCanJump = data.Bool("playerCanJump");
        RefillExtraJump = data.Bool("refillExtraJump");
        
        // color
        color = data.HexColor("color");
        var dd = DynamicData.For(this);
        var surfaces = dd.Get<List<Surface>>("Surfaces");
        foreach (Surface surface in surfaces)
        {
            var d = DynamicData.For(surface);
            var mesh = d.Get<VertexPositionColor[]>("mesh");
            int num1 = (int)(surface.Width / 4.0);
            int num2 = (int)(surface.Width * 0.20000000298023224);
            int surfaceStartIndex = (num1 + num2) * 6;
            for (int i = 0; i < num1 * 6; ++i)
                mesh[i].Color = color * 0.3f;
            for (int i = surfaceStartIndex; i < surfaceStartIndex + num1 * 6; ++i)
                mesh[i].Color = color * 0.8f;
        }

    }


    public CustomWater(Vector2 position, bool topSurface, bool bottomSurface, float width, float height) : base(
        position, topSurface, bottomSurface, width, height
    )
    {
    }

    public override void Update()
    {
        Color preRayTopColor = RayTopColor;
        RayTopColor = color * 0.6f;
        base.Update();
        RayTopColor = preRayTopColor;

        Player player = CollideFirst<Player>();
        if (player != null)
        {
            if (KillPlayer)
            {
                killPlayerTimer -= Engine.DeltaTime;
                if (killPlayerTimer <= 0)
                    player.Die(Vector2.Zero);
            }

            if (RefillExtraJump)
            {
                var jumpCount = ExtendedVariantsModule.Instance.VariantHandlers[ExtendedVariantsModule.Variant.JumpCount];
                ((JumpCount)jumpCount).RefillJumpBuffer();
                // typeof(JumpCount).GetMethod("RefillJumpBuffer").Invoke(jumpCount, Type.EmptyTypes);
            }
                
        }
        else
        {
            killPlayerTimer = KillPlayerDelay;
        }
    }

    public override void Render()
    {
        Color preFillColor = FillColor;
        FillColor = color * 0.3f;
        base.Render();
        FillColor = preFillColor;
    }
}