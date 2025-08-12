using LuckyHelper.Modules;
using LuckyHelper.Utils;

namespace LuckyHelper.Triggers;

public enum EntityTriggerMode
{
    OnEntityEnter,
    OnEntityStay,
    OnEntityLeave,
    Always
}

public abstract class EntityTrigger : Trigger
{
    public EntityTriggerMode EntityTriggerMode;
    
    private HashSet<string> briefTypes = new();

    private HashSet<Entity> preCollidedEntities = new();

    private bool hasPlayer;
    
    

    public EntityTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        EntityTriggerMode = data.Enum<EntityTriggerMode>("entityTriggerMode", EntityTriggerMode.OnEntityEnter);
        briefTypes = ParseUtils.ParseTypesStringToBriefNames(data.Attr("types", "Player"));
        Depth = -1000000;

        if (briefTypes.Contains(nameof(Player)))
        {
            hasPlayer = true;
            briefTypes.Remove(nameof(Player));
        }
    }
    public abstract void OnTriggered();

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

        if (EntityTriggerMode == EntityTriggerMode.Always)
            OnTriggered();
    }

    public virtual void OnCustomEnter(Entity entity)
    {
        if (EntityTriggerMode == EntityTriggerMode.OnEntityEnter)
            OnTriggered();
    }

    public virtual void OnCustomLeave(Entity entity)
    {
        if (EntityTriggerMode == EntityTriggerMode.OnEntityLeave)
            OnTriggered();
    }

    public virtual void OnCustomStay(Entity entity)
    {
        if (EntityTriggerMode == EntityTriggerMode.OnEntityStay)
            OnTriggered();
    }

    
    private bool EntityInsideTrigger(Entity entity)
    {
        if (entity.Collider == null)
        {
            return entity.X < Right && entity.X > Left && entity.Y < Bottom && entity.Y > Top;
        }

        return CollideCheck(entity);
    }



    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        if (hasPlayer)
            OnCustomEnter(player);
    }

    public override void OnStay(Player player)
    {
        base.OnStay(player);
        if (hasPlayer)
            OnCustomStay(player);
    }

    public override void OnLeave(Player player)
    {
        base.OnLeave(player);
        if (hasPlayer)
            OnCustomLeave(player);
    }
}