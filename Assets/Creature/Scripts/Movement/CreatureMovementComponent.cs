using System;
using System.Collections.Generic;
using System.Linq;
using Creature.Scripts.Animation;
using Creature.Scripts.Attributes;
using UnityEngine;
using UnityEngine.Assertions;

namespace Creature.Scripts.Movement
{
    public class PhysicsTickContext
    {
        public readonly CreatureMovementComponent MovementComponent;
        public Vector2 DesiredVelocity { get; private set; }

        public PhysicsTickContext(CreatureMovementComponent movementComponent)
        {
            MovementComponent = movementComponent;
            DesiredVelocity = new Vector2(0.0f, 0.0f);
        }

        public void AddImpulse(Vector2 velocity)
        {
            MovementComponent.AddImpulse(velocity);
        }

        public void AddDesiredVelocity(Vector2 velocity)
        {
            DesiredVelocity += velocity;
        }
        
        public void AddMinimumDesiredVelocity(Vector2 minimumVelocity)
        {
            Vector2 desiredVelocity = DesiredVelocity;
            desiredVelocity.x = Mathf.Max(desiredVelocity.x, minimumVelocity.x);
            desiredVelocity.y = Mathf.Max(desiredVelocity.y, minimumVelocity.y);
            DesiredVelocity = desiredVelocity;
        }
    }
    
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CreatureAttributeManager))]
    public class CreatureMovementComponent : MonoBehaviour, IAnimationControllerCallback
    {
        public static readonly CreatureAttribute MovementSpeedAttribute = new("movement.speed", 10.0f);
        public static readonly CreatureAttribute GravityAttribute = new("movement.gravity", -100.0f);
        
        public static readonly CreatureAttribute JumpCountAttribute = new("movement.jump_count", 1.0f);
        public static readonly CreatureAttribute JumpVelocityAttribute = new("movement.jump_velocity", 10.0f);

        [SerializeField] private float baseMovementSpeed = 10.0f;
        [SerializeField] private float baseGravity = -100.0f;
        [SerializeField] private int baseJumpCount = 1;
        [SerializeField] private float baseJumpVelocity = 10.0f;

        [SerializeField] private ContactFilter2D groundContactFilter;
        [SerializeField] private float baseAccelerationSpeed = 20.0f;
        [SerializeField] private float baseSlowdownSpeed = 100.0f;
        [SerializeField] private float exponentialSlowdownSpeed = 0.0f;
        [SerializeField] private float verticalAccelerationSpeed = 200.0f;
        [SerializeField] private float airborneMovementSpeedMultiplier = 0.5f;
        [SerializeField] private float jumpCooldown = 0.1f;

        [SerializeField] private string horizVelocityAnimParam = "horizontalVelocity";
        [SerializeField] private string verticalVelocityAnimParam = "verticalVelocity";
        [SerializeField] private string turnDirectionAnimParam = "turnDirection";
        [SerializeField] private string onGroundAnimParam = "grounded";
        [SerializeField] private string jumpAnimTrigger = "creatureJump";
        
        protected CreatureAttributeManager AttributeManager;
        protected Rigidbody2D Rigidbody;

        private CreatureAttributeInstance _movementSpeedInstance;
        private CreatureAttributeInstance _gravityInstance;

        private CreatureAttributeInstance _jumpCountInstance;
        private CreatureAttributeInstance _jumpVelocityInstance;

        private int _turnDirection;
        private int defaultTurnDirection = 1;
        private float turnDirectionVelocityThreshold = 0.1f;

        private Vector2 _inputAcceleration = new Vector2(0.0f, 0.0f);
        private float _inputAccelerationThreshold = 0.01f;
        private float _jumpInputThreshold = 0.5f;

        private int _currentJumpCounter = 0;
        private float _jumpRemainingCooldown = 0.0f;
        private bool _pendingJumpAnimTrigger = false;

        private readonly List<ICreatureMovementCallback> _callbacks = new();

        /** Current velocity of the player */
        public Vector2 Velocity => Rigidbody.velocity;
        /** True if the player is currently touching the ground */
        public bool IsOnGround => Rigidbody.IsTouching(groundContactFilter);
        /** Current direction the player is facing. 1 is +X, -1 is -X */
        public int TurnDirection => _turnDirection;

        public CreatureAttributeInstance MovementSpeedInstance => _movementSpeedInstance;
        public CreatureAttributeInstance GravityInstance => _gravityInstance;

        public void RegisterCallback(ICreatureMovementCallback callback)
        {
            _callbacks.Add(callback);
        }

        public void RemoveCallback(ICreatureMovementCallback callback)
        {
            _callbacks.Remove(callback);
        }
        
        /**
         * Instantly overwrites the current velocity of the object
         * Can be used at any point of time, not just inside of the FixedUpdate
         */
        public void SetVelocity(Vector2 velocity)
        {
            Rigidbody.velocity = velocity;
        }

        /**
         * Applies an instant impulse of velocity to the character
         * This will be directly applied to the velocity and will be gradually faded out across time
         * Can be used outside of physics tick
         */
        public void AddImpulse(Vector2 velocity)
        {
            Rigidbody.velocity = Rigidbody.velocity + velocity;
        }

        /** Updates acceleration input supplied by the input component every tick */
        public void SetInputAcceleration(Vector2 input)
        {
            _inputAcceleration = input;
        }

        protected bool CanMovementComponentUpdate()
        {
            return !ReferenceEquals(AttributeManager, null) && !ReferenceEquals(Rigidbody, null);
        }
        
        protected virtual void Start()
        {
            AttributeManager = GetComponent<CreatureAttributeManager>();
            Rigidbody = GetComponent<Rigidbody2D>();

            if (AttributeManager == null)
            {
                Debug.LogErrorFormat(this, "CharacterMovementComponent requires CharacterAttributeManager");
            }
            if (Rigidbody == null)
            {
                Debug.LogErrorFormat(this, "Rigidbody2D requires CharacterAttributeManager");
            }

            if (CanMovementComponentUpdate())
            {
                _movementSpeedInstance = AttributeManager.RegisterAttribute(MovementSpeedAttribute, baseMovementSpeed);
                _gravityInstance = AttributeManager.RegisterAttribute(GravityAttribute, baseGravity);
                _jumpCountInstance = AttributeManager.RegisterAttribute(JumpCountAttribute, baseJumpCount);
                _jumpVelocityInstance = AttributeManager.RegisterAttribute(JumpVelocityAttribute, baseJumpVelocity);
            }
            
            //Find callbacks among components and register them
            foreach (var movementComponentCallback in GetComponents<ICreatureMovementCallback>())
            {
                RegisterCallback(movementComponentCallback);
            }
        }

        protected virtual void FixedUpdate()
        {
            if (CanMovementComponentUpdate())
            {
                TickPhysics();
            }
        }

        protected virtual void TickPhysicsInput(PhysicsTickContext context)
        {
            if (IsOnGround)
            {
                if (_currentJumpCounter > 0)
                {
                    _currentJumpCounter = 0;
                    _jumpRemainingCooldown = jumpCooldown;
                }
                else if (_jumpRemainingCooldown > 0.0f)
                {
                    _jumpRemainingCooldown -= Math.Min(_jumpRemainingCooldown, Time.deltaTime);
                }
            }
            if (Mathf.Abs(_inputAcceleration.x) >= _inputAccelerationThreshold)
            {
                float forwardForceInput = _movementSpeedInstance.Value * _inputAcceleration.x;
                if (!IsOnGround)
                {
                    forwardForceInput *= airborneMovementSpeedMultiplier;
                }
                context.AddDesiredVelocity(new Vector2(forwardForceInput, 0.0f));
            }

            if (Mathf.Abs(_inputAcceleration.y) >= _inputAccelerationThreshold)
            {
                if (_inputAcceleration.y >= _jumpInputThreshold)
                {
                    DoJump(context);
                }
            }
        }

        protected virtual void DoJump(PhysicsTickContext context)
        {
            if (_currentJumpCounter < _jumpCountInstance.Value && _jumpRemainingCooldown <= 0.0f)
            {
                _currentJumpCounter++;
                context.AddImpulse(new Vector2(0.0f, _jumpVelocityInstance.Value));
                OnJump();
            }
        }

        protected virtual void OnJump()
        {
            _pendingJumpAnimTrigger = true;
        }

        protected virtual void TickPhysicsTurnDirection()
        {
            if (_turnDirection == 0)
            {
                _turnDirection = defaultTurnDirection;
            }

            Vector2 velocity = Rigidbody.velocity;
            if (Mathf.Abs(velocity.x) >= turnDirectionVelocityThreshold)
            {
                int newTurnDirection = Math.Sign(velocity.x);
               
                if (newTurnDirection != _turnDirection)
                {
                    _turnDirection = newTurnDirection;
                    OnTurnDirectionChange();
                }
            }
        }

        protected virtual void OnTurnDirectionChange()
        {
            _callbacks.ForEach(x => x.OnTurnDirectionChange(this));
        }

        protected virtual void TickPhysicsGravity(PhysicsTickContext context)
        {
            if (!IsOnGround)
            {
                context.AddDesiredVelocity(new Vector2(0.0f, _gravityInstance.Value));
            }
        }
        
        protected virtual void DoTickPhysics(PhysicsTickContext context)
        {
            TickPhysicsInput(context);
            TickPhysicsGravity(context);
            _callbacks.ForEach(x => x.OnPhysicsTick(context));
        }

        protected virtual void ApplyVelocityChange(Vector2 desiredVelocity)
        {
            Vector2 currentVelocity = Rigidbody.velocity;

            float horizVelocityDiff = desiredVelocity.x - currentVelocity.x;
            float maxHorizVelocityChangeRate = baseAccelerationSpeed;

            bool bIsSlowingDown = Math.Sign(desiredVelocity.x) != Math.Sign(currentVelocity.x) ||
                                  Mathf.Abs(desiredVelocity.x) < Mathf.Abs(currentVelocity.x);
            if (bIsSlowingDown)
            {
                //Take the biggest possible change between normal slowdown speed and exponential slowdown speed
                maxHorizVelocityChangeRate = Mathf.Max(baseSlowdownSpeed, Mathf.Abs(horizVelocityDiff) * exponentialSlowdownSpeed);
            }
            float horizVelocityChangeThisTick = Mathf.Min(Mathf.Abs(horizVelocityDiff), maxHorizVelocityChangeRate * Time.deltaTime);

            float verticalVelocityDiff = desiredVelocity.y - currentVelocity.y;
            float maxVerticalVelocityChangeRate = verticalAccelerationSpeed;
            float verticalVelocityChangeThisTick = Mathf.Min(Mathf.Abs(verticalVelocityDiff), maxVerticalVelocityChangeRate * Time.deltaTime);
            
            Vector2 newVelocity = currentVelocity + new Vector2(
                Mathf.Sign(horizVelocityDiff) * horizVelocityChangeThisTick, 
                Mathf.Sign(verticalVelocityDiff) * verticalVelocityChangeThisTick);
            Rigidbody.velocity = newVelocity;
        }

        protected virtual void PostPhysicsTick()
        {
            TickPhysicsTurnDirection();
            _callbacks.ForEach(x => x.PostPhysicsTick(this));
        }

        private void TickPhysics()
        {
            var physicsTickContext = new PhysicsTickContext(this);
            DoTickPhysics(physicsTickContext);
           
            ApplyVelocityChange(physicsTickContext.DesiredVelocity);
            PostPhysicsTick();
        }

        public void UpdateAnimatorParameters(Animator animator)
        {
            Vector2 velocity = Rigidbody.velocity;
            
            animator.SetInteger(turnDirectionAnimParam, TurnDirection);
            animator.SetBool(onGroundAnimParam, IsOnGround);
            
            animator.SetFloat(horizVelocityAnimParam, velocity.x);
            animator.SetFloat(verticalVelocityAnimParam, velocity.y);

            if (_pendingJumpAnimTrigger)
            {
                _pendingJumpAnimTrigger = false;
                animator.SetTrigger(jumpAnimTrigger);
            }
        }
    }
}