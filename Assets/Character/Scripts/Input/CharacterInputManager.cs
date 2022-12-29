using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Character.Scripts.Input
{
    [RequireComponent(typeof(CharacterInputComponent))]
    public class CharacterInputManager : MonoBehaviour
    {
        private readonly List<CharacterInputComponent> _inputComponents = new();
        private CharacterInputComponent _activeInputComponent;
        private int _disableInputStack = 0;

        public List<CharacterInputComponent> AllInputComponents => _inputComponents.ToList();
        public CharacterInputComponent ActiveInputComponent => _activeInputComponent;
        public bool InputDisabled => _disableInputStack > 0;
        
        public void DisableInput()
        {
            _disableInputStack++;
        }

        public void EnableInput()
        {
            --_disableInputStack;
        }

        private void Start()
        {
            _inputComponents.AddRange(GetComponents<CharacterInputComponent>());
            SelectActiveInputComponent();
        }

        private void Update()
        {
            if (!ReferenceEquals(_activeInputComponent, null) && !InputDisabled)
            {
                _activeInputComponent.ProcessInput();
            }
        }

        public void SelectActiveInputComponent()
        {
            var inputComponents = _inputComponents
                .Where(x => x.CanUseInputComponent())
                .OrderByDescending(x => x.GetInputComponentPriority())
                .ToList();

            if (inputComponents.Count == 0)
            {
                Debug.LogErrorFormat(this, "Failed to find usable InputComponent, none of the existing components {} are applicable", _inputComponents);
                return;
            }
            ForceActivateInputComponent(inputComponents[0]);
        }

        public void ForceActivateInputComponent(CharacterInputComponent inputComponent)
        {
            if (_activeInputComponent != inputComponent)
            {
                if (_activeInputComponent)
                {
                    _activeInputComponent.OnInputComponentStateChanged(false);
                }
                _activeInputComponent = inputComponent;
                if (inputComponent)
                {
                    inputComponent.OnInputComponentStateChanged(true);
                }
            }
        }
    }
}