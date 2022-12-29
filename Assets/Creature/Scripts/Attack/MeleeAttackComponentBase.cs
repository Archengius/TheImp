using System.Collections.Generic;
using Character.Scripts.Health;
using Creature.Scripts.Animation;
using Creature.Scripts.Health;
using UnityEngine;

namespace Creature.Scripts.Attack
{
    public abstract class MeleeAttackComponentBase : MonoBehaviour, IAnimationControllerCallback
    {
        [SerializeField] protected int damagePerAttack = 1;
        [SerializeField] protected bool canBeBlocked = true;
        [SerializeField] protected string attackSuccessTriggerName = "meleeAttackSuccess";
        
        private bool _pendingAttackSucessAnim = false;

        protected abstract Collider2D GetAttackZoneComponent();
        
        protected virtual bool CanTargetObject(DamageReceiverComponent component)
        {
            return true;
        }

        /** Immediately performs attack, completely ignoring the cooldown or enabled status */
        protected virtual bool DoAttack()
        {
            var activeContacts = new List<Collider2D>();
            var attackZone = GetAttackZoneComponent();
            if (attackZone == null)
            {
                return false;
            }
            attackZone.GetContacts(activeContacts);
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
            _pendingAttackSucessAnim = true;
        }

        public void UpdateAnimatorParameters(IAnimatorInstance animator)
        {
            if (_pendingAttackSucessAnim)
            {
                _pendingAttackSucessAnim = false;
                animator.SetTrigger(attackSuccessTriggerName);
            }
        }
    }
}