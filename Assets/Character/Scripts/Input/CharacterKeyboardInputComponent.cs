using System;
using System.Collections.Generic;
using Character.Scripts.Ability;
using UnityEngine;

namespace Character.Scripts.Input
{
    [Serializable]
    public struct AbilityKeyMapping
    {
        /** Key mapping associated with the ability */
        [SerializeField] public KeyCode keyMapping;
        /** Ability to activate */
        [SerializeField] public CharacterAbility ability;
        /** Whenever the ability should be stopped on button release */
        [SerializeField] public bool stopOnRelease;
    }

    [Serializable]
    public struct InputKeyMapping
    {
        [SerializeField] public KeyCode keyMapping;
        [SerializeField] public string axisNameMapping;
        
        /** Acceleration associated with this key */   
        [SerializeField] public Vector2 inputAcceleration;
    }
    
    /** Input component for the character that handles keyboard inputs */
    public class CharacterKeyboardInputComponent : CharacterInputComponent
    {
        /** Key mappings for input */
        [SerializeField] private List<InputKeyMapping> inputKeyMappings = new();
        
        /** Key mappings used for activating abilities */
        [SerializeField] private List<AbilityKeyMapping> abilityKeyMappings = new();

        /** Minimum threshold for input events */
        [SerializeField] private float inputSensitivityThreshold = 0.1f;

        public override void ProcessInput()
        {
            if (MovementComponent != null)
            {
                Vector2 resultAcceleration = new Vector2(0.0f, 0.0f);

                foreach (var keyMapping in inputKeyMappings)
                {
                    if (UnityEngine.Input.GetKey(keyMapping.keyMapping))
                    {
                        resultAcceleration += keyMapping.inputAcceleration;
                    }
                    else if (!string.IsNullOrEmpty(keyMapping.axisNameMapping))
                    {
                        float axisInput = UnityEngine.Input.GetAxis(keyMapping.axisNameMapping);
                        if (Mathf.Abs(axisInput) >= inputSensitivityThreshold)
                        {
                            resultAcceleration += keyMapping.inputAcceleration * axisInput;
                        }
                    }
                }
                MovementComponent.SetInputAcceleration(resultAcceleration);
            }
            
            if (AbilityManager != null)
            {
                if (AbilityManager.GetActiveAbility() == null)
                {
                    CharacterAbility requestedAbility = null;

                    foreach (var abilityKeyMapping in abilityKeyMappings)
                    {
                        if (UnityEngine.Input.GetKeyDown(abilityKeyMapping.keyMapping) &&
                            abilityKeyMapping.ability != null &&
                            abilityKeyMapping.ability.CanActivateAbility())
                        {
                            requestedAbility = abilityKeyMapping.ability;
                            break;
                        }
                    }

                    if (requestedAbility != null)
                    {
                        AbilityManager.ActivateAbility(requestedAbility);
                    }
                }
                else
                {
                    CharacterAbility activeAbility = AbilityManager.GetActiveAbility();
                    
                    foreach (var abilityKeyMapping in abilityKeyMappings)
                    {
                        if (abilityKeyMapping.ability == activeAbility &&
                            abilityKeyMapping.stopOnRelease &&
                            UnityEngine.Input.GetKeyUp(abilityKeyMapping.keyMapping))
                        {
                            AbilityManager.StopActiveAbility(false);
                            break;
                        }
                    }
                }
            }
        }
        
        public override int GetInputComponentPriority()
        {
            if (Application.isEditor)
            {
                return 100;
            }
            string platform = Application.platform.ToString();
            if (platform.Contains("Windows") || platform.Contains("Linux") || platform.Contains("OSX"))
            {
                return 50;
            }
            return -50;
        }

        public override bool CanUseInputComponent()
        {
            return true;
        }
    }
}