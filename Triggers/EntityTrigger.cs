using Celeste.Mod.Entities;
using LuckyHelper.Modules;
using LuckyHelper.Utils;

namespace LuckyHelper.Triggers;

public abstract class EntityTrigger : Trigger
{
    private HashSet<string> briefTypes = new();
    
    private HashSet<Entity> preCollidedEntities = new();
    
    public EntityTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        briefTypes = ParseUtils.ParseTypesStringToBriefNames(data.Attr("types"));
        Depth = -1000000;
    }

    public override void Update()
    {
        base.Update();

        HashSet<Entity> curCollidedEntities = new HashSet<Entity>();
        foreach (Entity entity in TypeToObjectsModule.IterEntitiesByTypes(briefTypes))
        {
            if (EntityInsideTrigger(entity))
            {
                curCollidedEntities.Add(entity);
                if (preCollidedEntities.Contains(entity))
                {
                    OnCustomStay(entity);
                }
                else
                {
                    OnCustomEnter(entity);
                }
            }
            else if (preCollidedEntities.Contains(entity))
            {
                OnCustomLeave(entity);
            }
        }

        preCollidedEntities = curCollidedEntities;
    }

    private bool EntityInsideTrigger(Entity entity)
    {
        if (entity.Collider == null)
        {
            return entity.X < Right && entity.X > Left && entity.Y < Bottom && entity.Y > Top;
        }

        return CollideCheck(entity);
    }


    public abstract void OnCustomEnter(Entity entity);
    public abstract void OnCustomLeave(Entity entity);
    public abstract void OnCustomStay(Entity entity);
}