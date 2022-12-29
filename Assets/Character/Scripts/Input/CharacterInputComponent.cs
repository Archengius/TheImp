using Character.Scripts.Ability;
using UnityEngine;

namespace Character.Scripts.Input
{
    /** Input component for the character */
    [RequireComponent(typeof(CharacterMovementComponent))]
    [RequireComponent(typeof(CharacterAbilityManager))]
    public abstract class CharacterInputComponent : MonoBehaviour
    {
        protected CharacterMovementComponent MovementComponent { get; private set; }
        protected CharacterAbilityManager AbilityManager { get; private set; }

        private void Start()
        {
            MovementComponent = GetComponent<CharacterMovementComponent>();
            AbilityManager = GetComponent<CharacterAbilityManager>();

            if (MovementComponent == null)
            {
                Debug.LogErrorFormat(this, "Failed to find CharacterMovementComponent on the game object");
            }
            if (MovementComponent == null)
            {
                Debug.LogErrorFormat(this, "Failed to find CharacterAbilityManager on the game object");
            }
        }

        /** Returns the priority of this input component */
        public abstract int GetInputComponentPriority();

        /** Return true if this input component can be used in the current configuration */
        public abstract bool CanUseInputComponent();

        public virtual void OnInputComponentStateChanged(bool newEnabled)
        {
        }

        /** Called to process input from this movement component */
        public abstract void ProcessInput();
    }
}