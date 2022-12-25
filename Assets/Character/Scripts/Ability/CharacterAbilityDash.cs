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
        private static readonly CreatureAttributeModifier GravityModifier = new(
            Guid.Parse("7691fa98-24fa-4893-be66-a9add71ac05f"), 
            "Dash Gravity Override",  
            (int) AttributeModifierPriority.ActiveAbility,
            AttributeModifierOperation.MultiplyTotal, 0.0f);
        
        [SerializeField] protected float dashVelocity = 500.0f;

        private CharacterInputManager _inputManager;
        private float initialVelocityX = 0.0f;

        protected override void Start()
        {
            base.Start();
            _inputManager = GetComponent<CharacterInputManager>();
        }

        public override void OnAbilityActivated()
        {
            base.OnAbilityActivated();
           
            if (_inputManager != null)
            {
                _inputManager.DisableInput();
            }
            if (MovementComponent != null)
            {
                initialVelocityX = MovementComponent.Velocity.x;
                MovementComponent.SetVelocity(new Vector2(initialVelocityX, 0.0f));
                MovementComponent.SetInputAcceleration(Vector2.zero);
                MovementComponent.GravityInstance.AddModifier(GravityModifier);
            }
        }

        public void OnPhysicsTick(PhysicsTickContext context)
        {
            if (IsAbilityActive)
            {
                Vector2 dashVelocityVector = new Vector2(Mathf.Sign(initialVelocityX) * dashVelocity, 0.0f);
                context.AddDesiredVelocity(dashVelocityVector);
            }
        }

        public override void OnAbilityStopped()
        {
            base.OnAbilityStopped();
           
            if (_inputManager != null)
            {
                _inputManager.EnableInput();
            }     
            if (MovementComponent != null)
            {
                var exitCharacterVelocity = Mathf.Sign(initialVelocityX) * MovementComponent.MovementSpeedInstance.Value;
                
                MovementComponent.SetVelocity(new Vector2(exitCharacterVelocity, 0.0f));
                MovementComponent.SetInputAcceleration(new Vector2(Mathf.Sign(initialVelocityX), 0.0f));
                MovementComponent.GravityInstance.RemoveModifier(GravityModifier);
            }
        }

        public bool AdjustDamage(DamageSource damageSource, ref int damageAmount)
        {
            //If the ability is in the main loop and the source is blockable, we block it
            return IsAbilityRunning && damageSource.CanBeBlocked;
        }
    }
}