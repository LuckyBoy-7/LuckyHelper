using Celeste.Mod.Entities;
using MonoMod.Utils;

namespace LuckyHelper.Entities;

[CustomEntity("LuckyHelper/DreamZone")]
[TrackedAs(typeof(DreamBlock))]
[Tracked]
public class DreamZone : DreamBlock
{
    public bool stopPlayerOnCollide = true;
    public bool killPlayerOnCollide = true;

    public DreamZone(EntityData data, Vector2 offset) : base(data, offset)
    {
        stopPlayerOnCollide = data.Bool("stopPlayerOnCollide");
        killPlayerOnCollide = data.Bool("killPlayerOnCollide");


        Collidable = false;
    }

    // public override void Added(Scene scene)
    // {
    //     base.Added(scene);
    //     var dd = DynamicData.For(this);
    //     DreamBlock. particles = dd.Get("particles"); 
    //     this.particles = new DreamBlock.DreamParticle[(int)((double)base.Width / 8.0 * ((double)base.Height / 8.0) * 0.699999988079071)];
    //     for (int i = 0; i < this.particles.Length; i++)
    //     {
    //         this.particles[i].Position = new Vector2(Calc.Random.NextFloat(base.Width), Calc.Random.NextFloat(base.Height));
    //         this.particles[i].Layer = Calc.Random.Choose(0, 1, 1, 2, 2, 2);
    //         this.particles[i].TimeOffset = Calc.Random.NextFloat();
    //         this.particles[i].Color = Color.LightGray * (0.5f + (float)this.particles[i].Layer / 2f * 0.5f);
    //         if (this.playerHasDreamDash)
    //         {
    //             switch (this.particles[i].Layer)
    //             {
    //                 case 0:
    //                     this.particles[i].Color = Calc.Random.Choose(Calc.HexToColor("FFEF11"), Calc.HexToColor("FF00D0"), Calc.HexToColor("08a310"));
    //                     break;
    //                 case 1:
    //                     this.particles[i].Color = Calc.Random.Choose(Calc.HexToColor("5fcde4"), Calc.HexToColor("7fb25e"), Calc.HexToColor("E0564C"));
    //                     break;
    //                 case 2:
    //                     this.particles[i].Color = Calc.Random.Choose(Calc.HexToColor("5b6ee1"), Calc.HexToColor("CC3B3B"), Calc.HexToColor("7daa64"));
    //                     break;
    //             }
    //         }
    //     }

// }

}