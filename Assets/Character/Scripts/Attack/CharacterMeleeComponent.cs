using System;
using Creature.Scripts.Animation;
using Creature.Scripts.Attack;
using Creature.Scripts.Movement;
using UnityEngine;

namespace Character.Attack
{
    [RequireComponent(typeof(CreatureMovementComponent))]
    public class CharacterMeleeComponent : MeleeAttackComponentBase, IAnimationControllerCallback
    {
        [SerializeField] private Collider2D colliderLeft;
        [SerializeField] private Collider2D colliderRight;

        private CreatureMovementComponent _movementComponent;

        protected virtual void Start()
        {
            _movementComponent = GetComponent<CreatureMovementComponent>();
        }


        protected override Collider2D GetAttackZoneComponent()
        {
            if (_movementComponent)
            {
                return _movementComponent.TurnDirection == 1 ? colliderRight : colliderLeft;
            }

            return colliderRight;
        }

        public void OnAnimNotifyTriggered(string notifyName, int customData)
        {
            if (notifyName == "characterAttackMelee")
            {
                DoAttack();
            }
        }
    }
}