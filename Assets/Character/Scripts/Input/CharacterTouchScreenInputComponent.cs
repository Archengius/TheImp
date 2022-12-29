using System;
using System.Collections.Generic;
using Character.Scripts.Ability;
using UnityEngine;

namespace Character.Scripts.Input {
public class CharacterTouchScreenInputComponent : CharacterInputComponent {
  
    [SerializeField] [InspectorName("Joystick")]
    private JoystickComponent joystick;

    [SerializeField] [InspectorName("Jump Button")]
    private ButtonComponent jumpButton;

    [SerializeField] [InspectorName("Dash Button")]
    private ButtonComponent dashButton;
    [SerializeField] [InspectorName("Attack Button")] private ButtonComponent attackButton;

    [SerializeField] [InspectorName("Dash Ability")]
    private CharacterAbilityDash dashAbility;
    [SerializeField] [InspectorName("Attack Ability")]
    private CharacterAbility attackAbility;

    [SerializeField] private bool editorPriority = false;

    public override void OnInputComponentStateChanged(bool newEnabled)
    {
        if (joystick)
        {
            joystick.enabled = newEnabled;
        }

        if (jumpButton)
        {
            jumpButton.enabled = newEnabled;
        }

        if (dashButton)
        {
            dashButton.enabled = newEnabled;
        }

        if (attackButton)
        {
            attackButton.enabled = newEnabled;
        }
    }

    public override void ProcessInput() {
        if (MovementComponent != null) {
            Vector2 resultAcceleration = new Vector2(0.0f, 0.0f);

            resultAcceleration += joystick.inputValue;

            if (jumpButton.isPressed) {
                resultAcceleration.y = 1f;
            }

            MovementComponent.SetInputAcceleration(resultAcceleration);
        }

        if (AbilityManager != null) {
            if (dashButton != null && dashAbility != null && dashButton.isPressed) {
                AbilityManager.ActivateAbility(dashAbility);
            }
            if (attackButton != null && attackAbility != null && attackButton.isPressed) {
                AbilityManager.ActivateAbility(attackAbility);
            }
        }
    }

    public override int GetInputComponentPriority() {
        if (Application.isEditor && editorPriority) {
            return 110;
        }

        string platform = Application.platform.ToString();
        if (platform.Contains("Android")) {
            return 60;
        }

        return -50;
    }

    public override bool CanUseInputComponent() {
        return true;
    }
}
}