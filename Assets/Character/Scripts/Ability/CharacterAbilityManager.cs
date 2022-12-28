using System;
using System.Collections.Generic;
using System.Linq;
using Creature.Scripts.Animation;
using UnityEngine;

namespace Character.Scripts.Ability
{
    public class CharacterAbilityManager : MonoBehaviour, IAnimationControllerCallback
    {
        private readonly List<CharacterAbility> _abilities = new();
        private CharacterAbility _activeAbility;

        public List<CharacterAbility> Abilities => _abilities.ToList();
        public CharacterAbility ActiveAbility => _activeAbility;

        private void Start()
        {
            _abilities.AddRange(GetComponents<CharacterAbility>());
            _abilities.ForEach(x => x.SetAbilityRegistered(this));
        }

        private void Update()
        {
            if (!ReferenceEquals(_activeAbility, null))
            {
                _activeAbility.OnAbilityTick(Time.deltaTime);
            }

            foreach (var ability in _abilities)
            {
                if (ability != _activeAbility)
                {
                    ability.OnNonActiveAbilityTick(Time.deltaTime);
                }
            }
        }
        
        public CharacterAbility GetActiveAbility()
        {
            return _activeAbility;
        }
        
        public bool CanActivateAbility(CharacterAbility characterAbility)
        {
            return !ReferenceEquals(characterAbility, null) &&
                   characterAbility.GetAbilityManager() == this &&
                   characterAbility.CanActivateAbility();
        }
        
        public bool ActivateAbility(CharacterAbility characterAbility) {
            if (!CanActivateAbility(characterAbility))
            {
                return false;
            }

            if (GetActiveAbility() == characterAbility)
            {
                return true;
            }
            if (!ReferenceEquals(GetActiveAbility(), null)) 
            {
                StopActiveAbility(true);
            }
            _activeAbility = characterAbility;
            _activeAbility.OnAbilityActivated();
            return true;
        }

        public void StopActiveAbility(bool force)
        {
            if (!ReferenceEquals(GetActiveAbility(), null))
            {
                _activeAbility.OnAbilityStopped();
                _activeAbility = null;
            }
        }

        public AnimationControllerOverride GetAnimationControllerOverride()
        {
            if (_activeAbility)
            {
                var controllerOverride = _activeAbility.GetAnimControllerOverride();
                if (controllerOverride)
                {
                    return new AnimationControllerOverride(controllerOverride,
                        AnimControllerOverridePriority.ActiveAbility);
                }
            }
            return new AnimationControllerOverride(null);
        }

        public void UpdateAnimatorParameters(IAnimatorInstance animator)
        {
            if (_activeAbility)
            {
                _activeAbility.UpdateAnimatorParameters(animator);
            }
        }
    }
}