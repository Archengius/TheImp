using System;
using Character.Scripts.Input;
using Creature.Scripts.Attributes;
using Creature.Scripts.Health;
using Creature.Scripts.Movement;
using UnityEngine;

namespace Character.Scripts.Ability
{
    [RequireComponent(typeof(CharacterInputManager))]
    public class CharacterAbilityDash : TimedCharacterAbility, IHealthComponentCallback, ICreatureMovementCallback
    {
        [SerializeField] protected float dashVelocity = 500.0f;
        private float initialVelocityX = 0.0f;

        public override void OnAbilityActivated()
        {
            base.OnAbilityActivated();
            
            if (MovementComponent != null)
            {
                initialVelocityX = MovementComponent.Velocity.x;
                if (initialVelocityX == 0.0f)
                {
                    initialVelocityX = MovementComponent.TurnDirection;
                }
                MovementComponent.SetVelocity(new Vector2(initialVelocityX, 0.0f));
                MovementComponent.SetInputAcceleration(Vector2.zero);
                MovementComponent.DisableInputAccelerationMovement();
            }
        }

        protected Vector2 GetDesiredVelocity()
        {
            var gravity = MovementComponent.GravityInstance.Value;
            var enterCharacterVelocity = new Vector2(initialVelocityX, gravity);
            var dashVelocityVector = new Vector2(Mathf.Sign(initialVelocityX) * dashVelocity, 0.0f);
            var exitCharacterVelocity = new Vector2(Mathf.Sign(initialVelocityX) * MovementComponent.MovementSpeedInstance.Value, gravity);
            
            if (IsAbilityEnter)
            {
                //translate from enterCharacterVelocity to dashVelocityVector
                return Vector2.Lerp(enterCharacterVelocity, dashVelocityVector, AbilityEnterProgress);
            }
            else if (IsAbilityRunning)
            {
                //We're in full dash velocity now
                return dashVelocityVector;
            }
            else if (IsAbilityExit)
            {
                //translate from dashVelocity to exitCharacterVelocity
                return Vector2.Lerp(dashVelocityVector, exitCharacterVelocity, AbilityExitProgress);
            }
            return Vector2.zero;
        }

        public void OnPostPhysicsTick(PhysicsTickContext context)
        {
            Vector2 desiredVelocity = GetDesiredVelocity();
            if (IsAbilityActive)
            {
                context.SetVelocity(desiredVelocity);
            }
        }

        public override void OnAbilityStopped()
        {
            base.OnAbilityStopped();
            
            if (MovementComponent != null)
            {
                var gravity = MovementComponent.GravityInstance.Value;
                var exitCharacterVelocity = new Vector2(Mathf.Sign(initialVelocityX) * MovementComponent.MovementSpeedInstance.Value, gravity);

                MovementComponent.SetVelocity(exitCharacterVelocity);
                MovementComponent.SetInputAcceleration(new Vector2(Mathf.Sign(initialVelocityX), 0.0f));
                MovementComponent.EnableInputAccelerationMovement();
            }
        }

        public bool AdjustDamage(DamageSource damageSource, ref int damageAmount)
        {
            //If the ability is in the main loop and the source is blockable, we block it
            return IsAbilityRunning && damageSource.CanBeBlocked;
        }
    }
}