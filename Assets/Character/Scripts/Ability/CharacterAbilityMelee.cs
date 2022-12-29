using UnityEngine;

namespace Character.Scripts.Ability
{
    public class CharacterAbilityMelee : TimedCharacterAbility
    {
        public override void OnAbilityActivated()
        {
            base.OnAbilityActivated();
            MovementComponent.SetVelocity(Vector2.zero);
            MovementComponent.DisableInputAccelerationMovement();
        }

        public override void OnAbilityStopped()
        {
            base.OnAbilityStopped();
            MovementComponent.EnableInputAccelerationMovement();
        }
    }
}