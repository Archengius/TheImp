using System;
using System.Collections.Generic;
using Character.Scripts.Health;
using Creature.Scripts.Health;
using UnityEngine;

namespace Creature.Scripts.Attack
{
    public class MeleeAttackComponent : MonoBehaviour
    {
        [SerializeField] protected Collider2D colliderComponent;
        [SerializeField] protected ContactFilter2D contactFilter;
        [SerializeField] protected int damagePerAttack = 1;
        [SerializeField] protected bool canBeBlocked = true;
        [SerializeField] protected float timeBetweenAttacks = 0.5f;
        [SerializeField] protected bool onlyAttackPlayer = true;
        [SerializeField] protected bool attackEnabled = true;

        protected float _timeBetweenFailedAttacks = 0.1f;
        protected float _currentAttackCooldown;

        public void SetAttackEnabled(bool newAttackEnabled)
        {
            attackEnabled = newAttackEnabled;
        }

        protected virtual bool CanTargetObject(DamageReceiverComponent component)
        {
            if (onlyAttackPlayer && component is not CharacterHealthComponent)
            {
                return false;
            }
            return true;
        }

        /** Immediately performs attack, completely ignoring the cooldown or enabled status */
        public bool DoAttack()
        {
            var activeContacts = new List<Collider2D>();
            colliderComponent.GetContacts(contactFilter, activeContacts);
            bool result = false;
            
            foreach (var contactCollider in activeContacts)
            {
                var damageReceiver = contactCollider.GetComponent<DamageReceiverComponent>();
                
                if (!ReferenceEquals(damageReceiver, null) && CanTargetObject(damageReceiver))
                {
                    var damageSource = DamageSource.CauseMeleeDamage(this, canBeBlocked);
                    result |= damageReceiver.AttackFrom(damageSource, damagePerAttack);
                }
            }
            if (result)
            {
                OnAttackSuccess();
            }
            return result;
        }

        protected virtual void OnAttackSuccess()
        {
        }

        protected virtual void Update()
        {
            if (!attackEnabled)
            {
                return;
            }
            if (_currentAttackCooldown > 0.0f)
            {
                _currentAttackCooldown -= Mathf.Min(_currentAttackCooldown, Time.deltaTime);
                return;
            }
            _currentAttackCooldown = DoAttack() ? timeBetweenAttacks : _timeBetweenFailedAttacks;
        }
    }
}