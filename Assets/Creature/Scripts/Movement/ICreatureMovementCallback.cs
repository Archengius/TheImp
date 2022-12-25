namespace Creature.Scripts.Movement
{
    public interface ICreatureMovementCallback
    {
        public virtual void OnPhysicsTick(PhysicsTickContext context)
        {
        }

        public virtual void PostPhysicsTick(CreatureMovementComponent component)
        {
        }

        public virtual void OnTurnDirectionChange(CreatureMovementComponent component)
        {
        }
    }
}