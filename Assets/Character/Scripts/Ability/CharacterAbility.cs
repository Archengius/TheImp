using UnityEngine;

namespace Character.Scripts.Ability
{
    [RequireComponent(typeof(CharacterAbilityManager))]
    public abstract class CharacterAbility : MonoBehaviour
    {
        private CharacterAbilityManager _abilityManager;
        public bool IsAbilityActive => _abilityManager.ActiveAbility == this;

        public CharacterAbilityManager GetAbilityManager()
        {
            return _abilityManager;
        }

        protected void StopAbility(bool force)
        {
            if (IsAbilityActive)
            {
                GetAbilityManager().StopActiveAbility(force);
            }
        }

        /** Return anim controller that should be handling character movement instead of a default controller */
        public virtual RuntimeAnimatorController GetAnimControllerOverride()
        {
            return null;
        }

        /** Update ability-specific animator parameters here, best used together with GetAnimControllerOverride */
        public virtual void UpdateAnimatorParameters(Animator mainAnimator)
        {
        }

        /** True if CharController::ActivateAbility can actually activate this ability */
        public virtual bool CanActivateAbility()
        {
            return true;
        }

        /** Called right after CharController::ActivateAbility */
        public virtual void OnAbilityActivated()
        {
        }

        /** Called every frame the ability is active */
        public virtual void OnAbilityTick(float dt)
        {
        }
        
        /** Called once the ability has been terminated through CharacterAbilityManager::StopActiveAbility */
        public virtual void OnAbilityStopped()
        {
        }

        /** Called right after the ability has been registered with the ability manager */
        protected virtual void OnAbilityRegistered()  {
        }
        
        internal void SetAbilityRegistered(CharacterAbilityManager characterAbilityManager)
        {
            _abilityManager = characterAbilityManager;
            OnAbilityRegistered();
        }

        public virtual bool RequestAbilityStop()
        {
            return true;
        }
    }
}