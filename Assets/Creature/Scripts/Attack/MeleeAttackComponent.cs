using System;
using System.Collections.Generic;
using Character.Scripts.Health;
using Creature.Scripts.Animation;
using Creature.Scripts.Health;
using UnityEngine;

namespace Creature.Scripts.Attack
{
    public class MeleeAttackComponent : MeleeAttackComponentBase
    {
        [SerializeField] protected Collider2D colliderComponent;
        [SerializeField] protected float timeBetweenAttacks = 0.5f;
        [SerializeField] protected bool onlyAttackPlayer = true;
        [SerializeField] protected bool attackEnabled = true;

        protected float _timeBetweenFailedAttacks = 0.1f;
        protected float _currentAttackCooldown;

        public void SetAttackEnabled(bool newAttackEnabled)
        {
            attackEnabled = newAttackEnabled;
        }

        protected override Collider2D GetAttackZoneComponent()
        {
            return colliderComponent;
        }

        protected override bool CanTargetObject(DamageReceiverComponent component)
        {
            if (onlyAttackPlayer && component is not CharacterHealthComponent)
            {
                return false;
            }
            return true;
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