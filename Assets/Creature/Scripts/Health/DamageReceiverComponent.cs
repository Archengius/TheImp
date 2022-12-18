using UnityEngine;

namespace Creature.Scripts.Health
{
    public abstract class DamageReceiverComponent : MonoBehaviour
    {
        public abstract bool CanTakeDamage(DamageSource source);
        
        public abstract bool AttackFrom(DamageSource source, int damageAmount);
    }
}