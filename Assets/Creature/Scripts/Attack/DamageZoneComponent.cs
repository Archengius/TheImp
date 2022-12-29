using System;
using Creature.Scripts.Health;
using UnityEngine;

namespace Creature.Scripts.Attack
{
    [RequireComponent(typeof(Collider2D))]
    public class DamageZoneComponent : MonoBehaviour
    {
        [SerializeField] protected int damageAmount = 1000;
        [SerializeField] protected bool canBeBlocked = false;
        
        private void OnTriggerEnter2D(Collider2D col)
        {
            DamageReceiverComponent damageReceiverComponent = col.GetComponent<DamageReceiverComponent>();
            if (damageReceiverComponent)
            {
                OnGameObjectEnter(damageReceiverComponent);
            }
        }

        protected virtual void OnGameObjectEnter(DamageReceiverComponent damageReceiverComponent)
        {
            damageReceiverComponent.AttackFrom(DamageSource.CauseEnvironmentDamage(canBeBlocked), damageAmount);
        }
    }
}