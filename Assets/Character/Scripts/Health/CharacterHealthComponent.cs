using System;
using Character.Scripts.Ability;
using Character.Scripts.Input;
using Creature.Scripts.Attributes;
using Creature.Scripts.Health;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Character.Scripts.Health
{
    [RequireComponent(typeof(CharacterInputManager))]
    [RequireComponent(typeof(CharacterAbilityManager))]
    public sealed class CharacterHealthComponent : CreatureHealthComponent
    {
        [SerializeField] private float invulnerabilityTimeOnHit = 1.0f;
        [SerializeField] private float retrySceneOpenDelay = 0.5f;
        [SerializeField] private string retrySceneName = "";

        private CharacterInputManager _characterInputManager;
        private CharacterAbilityManager _characterAbilityManager;
        private CharacterMovementComponent _characterMovementComponent;
        
        private float _currentInvulnerabilityTime = 0.0f;
        private float _currentDeathTime = 0.0f;

        public float CurrentInvulnerabilityTime => _currentInvulnerabilityTime;

        protected override void Start()
        {
            base.Start();
            _characterInputManager = GetComponent<CharacterInputManager>();
            _characterAbilityManager = GetComponent<CharacterAbilityManager>();
            _characterMovementComponent = GetComponent<CharacterMovementComponent>();
        }

        protected override void Update()
        {
            base.Update();
            if (RanOutOfHealth)
            {
                TickRanOutOfHealth(Time.deltaTime);
            }

            if (_currentInvulnerabilityTime > 0.0f)
            {
                _currentInvulnerabilityTime -= Mathf.Min(_currentInvulnerabilityTime, Time.deltaTime);
            }
        }

        private void TickRanOutOfHealth(float dt)
        {
            _currentDeathTime += dt;
            if (!(_currentDeathTime >= retrySceneOpenDelay)) return;
            
            //Open a retry scene if we actually have any
            if (!string.IsNullOrEmpty(retrySceneName))
            {
                SceneManager.LoadScene(retrySceneName, LoadSceneMode.Single);
            }
            else
            {
                //Otherwise just reload the current scene
                SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
            }
        }

        protected override void OnRanOutOfHealth()
        {
            base.OnRanOutOfHealth();
            if (_characterInputManager != null)
            {
                _characterInputManager.DisableInput();
            }

            if (_characterMovementComponent)
            {
                _characterMovementComponent.SetInputAcceleration(Vector2.zero);
                _characterMovementComponent.SetVelocity(Vector2.zero);
            }
            if (_characterAbilityManager != null)
            {
                _characterAbilityManager.StopActiveAbility(true);
            }
        }
        
        protected override void OnDamageTaken(DamageSource damageSource, int amount)
        {
            base.OnDamageTaken(damageSource, amount);
            _currentInvulnerabilityTime = invulnerabilityTimeOnHit;
        }

        public override bool CanTakeDamage(DamageSource source)
        {
            if (_currentInvulnerabilityTime > 0.0f && source.CanBeBlocked)
            {
                return false;
            }
            return base.CanTakeDamage(source);
        }
    }
}