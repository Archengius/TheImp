using Character.Scripts.Health;
using UnityEngine;

namespace Character.Scripts.Ability
{
    [RequireComponent(typeof(CharacterMovementComponent))]
    [RequireComponent(typeof(CharacterHealthComponent))]
    public abstract class TimedCharacterAbility : CharacterAbility
    {
        /** Base cooldown after the ability is finished before it can be used again */
        [SerializeField] protected float activationCooldown = 5.0f;
        
        [SerializeField] protected float abilityEnterTime = 0.25f;
        [SerializeField] protected float minAbilityActivationTime = 0.0f;
        [SerializeField] protected float maxAbilityActivationTime = 0.0f;
        [SerializeField] protected float abilityExitTime = 0.25f;

        [SerializeField] protected bool cancelOnDamage = false;
        [SerializeField] protected bool cancelOnMove = false;

        [SerializeField] protected float minVelocityToEnter = 0.0f;
        [SerializeField] protected float maxVelocityToEnter = 0.0f;
        
        protected CharacterMovementComponent MovementComponent;
        protected CharacterHealthComponent HealthComponent;

        private float _abilityCooldownTimer = 0.0f;
        private float _abilityActivationTime = 0.0f;
        private bool _requestedAbilityStop = false;
        private float _maxAbilityActivationTime = 0.0f;
        private bool _isCurrentlyEnteringAbility = false;
        private bool _isCurrentlyExitingAbility = false;

        public bool IsOnCooldown => _abilityCooldownTimer > 0.0f;
        public float AbilityActivationTimeFull => _abilityActivationTime;
        public float MaxAbilityActivationTimeFull => _maxAbilityActivationTime;
        public float AbilityEnterTime => abilityEnterTime;
        public float AbilityExitTime => abilityExitTime;
        public float MaxAbilityActivationTime => MaxAbilityActivationTimeFull - (AbilityEnterTime + AbilityExitTime);
        public float AbilityActivationTime => Mathf.Clamp(AbilityActivationTimeFull - AbilityEnterTime, 0.0f, MaxAbilityActivationTime);
        public bool IsAbilityEnter => IsAbilityActive && AbilityActivationTimeFull < AbilityEnterTime;
        public bool IsAbilityExit => IsAbilityActive && AbilityActivationTimeFull > (MaxAbilityActivationTimeFull - (AbilityEnterTime + AbilityExitTime));
        public bool IsAbilityRunning => IsAbilityActive && !IsAbilityEnter && !IsAbilityExit;
        
        protected virtual void Start()
        {
            MovementComponent = GetComponent<CharacterMovementComponent>();
            HealthComponent = GetComponent<CharacterHealthComponent>();
        }
        
        public override void OnAbilityActivated()
        {
            base.OnAbilityActivated();
            _abilityActivationTime = 0.0f;
            _isCurrentlyEnteringAbility = true;
            SetMaxActiveAbilityTime(Mathf.Max(minAbilityActivationTime, maxAbilityActivationTime));
        }
        
        public override void OnAbilityTick(float dt)
        {
            base.OnAbilityTick(dt);
            _abilityActivationTime += dt;

            //If we have requested ability stop and have passed the required minimum time, jump to the end
            if (_requestedAbilityStop && AbilityActivationTime >= minAbilityActivationTime)
            {
                _requestedAbilityStop = false;
                SetTimeToAbilityExit();
                return;
            }
            
            //Callbacks for enter end and exit start
            if (_isCurrentlyEnteringAbility && !IsAbilityEnter)
            {
                _isCurrentlyEnteringAbility = false;
                OnAbilityEnterEnd();
            }
            if (!_isCurrentlyExitingAbility && IsAbilityExit)
            {
                _isCurrentlyExitingAbility = true;
                OnAbilityExitStart();
            }

            //Tick different events for enter, exit and running
            if (IsAbilityEnter)
            {
                OnAbilityEnterTick(dt);    
            }
            else if (IsAbilityExit)
            {
                OnAbilityExitTick(dt);
            }
            else
            {
                OnAbilityRunningTick(dt);
            }
        }

        protected virtual bool CheckAbilityCancel()
        {
            if (cancelOnDamage && HealthComponent.CurrentInvulnerabilityTime > 0.0f)
            {
                return true;
            }
            if (cancelOnMove && !CheckActivationVelocity())
            {
                return true;
            }
            return false;
        }

        protected virtual void OnAbilityEnterTick(float dt)
        {
            if (CheckAbilityCancel())
            {
                StopAbility(true);
            }
        }

        protected virtual void OnAbilityRunningTick(float dt)
        {
        }

        protected virtual void OnAbilityExitTick(float dt)
        {
        }
        
        public override void OnAbilityStopped()
        {
            base.OnAbilityStopped();
            _abilityCooldownTimer = activationCooldown;
            _abilityActivationTime = 0.0f;
            _maxAbilityActivationTime = 0.0f;
            _requestedAbilityStop = false;
        }

        protected void SetTimeToAbilityExit()
        {
            _abilityActivationTime = _maxAbilityActivationTime - abilityExitTime;
        }

        protected void SetMaxActiveAbilityTime(float newMaxActiveAbilityTime)
        {
            //TODO: Clamp the time to make sure we are not bugging the enter/exit animation states?
            _maxAbilityActivationTime = newMaxActiveAbilityTime;
        }

        protected virtual bool CheckActivationVelocity()
        {
            float velocityMagnitude = MovementComponent.Velocity.magnitude;

            if (minVelocityToEnter != 0.0f && minVelocityToEnter > velocityMagnitude)
            {
                return false;
            }
            if (maxVelocityToEnter != 0.0f && maxVelocityToEnter < velocityMagnitude)
            {
                return false;
            }
            return true;
        }
        
        public override bool CanActivateAbility()
        {
            if (IsOnCooldown)
            {
                return false;
            }
            if (!CheckActivationVelocity())
            {
                return false;
            }
            return base.CanActivateAbility();
        }

        public override bool RequestAbilityStop()
        {
            //Can directly go and cancel the ability during the enter animation
            if (IsAbilityEnter)
            {
                return true;
            }
            //Cannot cancel ability during exit animation, it's already ending
            if (IsAbilityExit)
            {
                return false;
            }
            //Otherwise it is the main loop, mark as requested for next frame but prevent the cancel
            _requestedAbilityStop = true;
            return false;
        }
        
        protected virtual void OnAbilityEnterEnd()
        {
        }

        protected virtual void OnAbilityExitStart()
        {
        }
    }
}