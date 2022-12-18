using System;
using Creature.Scripts.Attributes;
using UnityEngine;
using UnityEngine.Assertions;

namespace Creature.Scripts.Movement
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CreatureAttributeManager))]
    public class CreatureMovementComponent : MonoBehaviour
    {
        public static readonly CreatureAttribute MovementSpeedAttribute = new("movement.speed", 100.0f);
        public static readonly CreatureAttribute GravityScaleAttribute = new("movement.gravity_scale", 1.0f);
        
        public static readonly CreatureAttribute JumpCountAttribute = new("movement.jump_count", 1.0f);
        public static readonly CreatureAttribute JumpVelocityAttribute = new("movement.jump_velocity", 100.0f);

        [SerializeField] private float baseMovementSpeed = 100.0f;
        [SerializeField] private float baseGravityScale = 1.0f;
        [SerializeField] private int baseJumpCount = 1;
        [SerializeField] private float baseJumpVelocity = 100.0f;

        private CreatureAttributeManager _attributeManager;
        private Rigidbody2D _rigidbody;
        private ContactFilter2D _groundContactFilter;
        
        private CreatureAttributeInstance _movementSpeedInstance;
        private CreatureAttributeInstance _gravityScaleInstance;

        private CreatureAttributeInstance _jumpCountInstance;
        private CreatureAttributeInstance _jumpVelocityInstance;

        private int _turnDirection;
        private int defaultTurnDirection = 1;
        private float turnDirectionVelocityThreshold = 0.1f;

        private Vector2 _inputAcceleration = new Vector2(0.0f, 0.0f);
        private float _inputAccelerationThreshold = 0.01f;
        private float _jumpInputThreshold = 0.5f;

        private int _currentJumpCounter = 0;

        private Vector2 _desiredVelocity = new Vector2(0.0f, 0.0f);

        /** Current velocity of the player */
        public Vector2 Velocity => _rigidbody.velocity;
        /** True if the player is currently touching the ground */
        public bool IsOnGround => _rigidbody.IsTouching(_groundContactFilter);
        /** Current direction the player is facing. 1 is +X, -1 is -X */
        public int TurnDirection => _turnDirection;

        public CreatureAttributeInstance MovementSpeedInstance => _movementSpeedInstance;
        public CreatureAttributeInstance GravityScaleInstance => _gravityScaleInstance;

        /**
         * Instantly overwrites the current velocity of the object
         * Can be used at any point of time, not just inside of the FixedUpdate
         */
        public void SetVelocity(Vector2 velocity)
        {
            _rigidbody.velocity = velocity;
        }

        /**
         * Applies an instant impulse of velocity to the character
         * This will be directly applied to the velocity and will be gradually faded out across time
         * Can be used outside of physics tick
         */
        public void AddImpulse(Vector2 velocity)
        {
            _rigidbody.velocity = _rigidbody.velocity + velocity;
        }

        /**
         * Adds desired velocity that the character will try to achieve during this physics tick
         * Will be reset every tick so to keep the velocity you have to actually apply movement
         */
        public void AddDesiredVelocity(Vector2 velocity)
        {
            _desiredVelocity += velocity;
        }
        
        
        /**
         * Adds minimum desired velocity to the character during this physics tick
         * That means it will only add the velocity if the current desired velocity is below the threshold
         */
        public void AddMinimumDesiredVelocity(Vector2 minimumVelocity)
        {
            Vector2 desiredVelocity = _desiredVelocity;
        
        }
        
        /** Updates acceleration input supplied by the input component every tick */
        public void SetInputAcceleration(Vector2 input)
        {
            _inputAcceleration = input;
        }

        protected bool CanMovementComponentUpdate()
        {
            return !ReferenceEquals(_attributeManager, null) && !ReferenceEquals(_rigidbody, null);
        }
        
        protected virtual void Start()
        {
            _attributeManager = GetComponent<CreatureAttributeManager>();
            _rigidbody = GetComponent<Rigidbody2D>();

            if (_attributeManager == null)
            {
                Debug.LogErrorFormat(this, "CharacterMovementComponent requires CharacterAttributeManager");
            }
            if (_rigidbody == null)
            {
                Debug.LogErrorFormat(this, "Rigidbody2D requires CharacterAttributeManager");
            }

            if (CanMovementComponentUpdate())
            {
                _movementSpeedInstance = _attributeManager.RegisterAttribute(MovementSpeedAttribute, baseMovementSpeed);
                _gravityScaleInstance = _attributeManager.RegisterAttribute(GravityScaleAttribute, baseGravityScale);
                _jumpCountInstance = _attributeManager.RegisterAttribute(JumpCountAttribute, baseJumpCount);
                _jumpVelocityInstance = _attributeManager.RegisterAttribute(JumpVelocityAttribute, baseJumpVelocity);

                if (_gravityScaleInstance != null)
                {
                    _gravityScaleInstance.ValueChanged += OnGravityScaleChanged;
                    OnGravityScaleChanged();
                } 
            }
        }
        
        protected virtual void FixedUpdate()
        {
            if (CanMovementComponentUpdate())
            {
                TickPhysics();
            }
        }

        protected virtual void TickPhysicsInput()
        {
            if (Mathf.Abs(_inputAcceleration.x) >= _inputAccelerationThreshold)
            {
                float forwardForceInput = _movementSpeedInstance.Value * _inputAcceleration.x;
                _rigidbody.AddForce(new Vector2(forwardForceInput, 0.0f));
            }

            if (Mathf.Abs(_inputAcceleration.y) >= _inputAccelerationThreshold)
            {
                if (_inputAcceleration.y >= _jumpInputThreshold)
                {
                    DoJump();
                }
            }
        }

        protected virtual void DoJump()
        {
            if (_currentJumpCounter < _jumpCountInstance.Value)
            {
                _currentJumpCounter++;
                AddVelocity(new Vector2(0.0f, _jumpVelocityInstance.Value));
            }
        }

        protected virtual void TickPhysicsTurnDirection()
        {
            if (_turnDirection == 0)
            {
                _turnDirection = defaultTurnDirection;
            }

            Vector2 velocity = _rigidbody.velocity;
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
        }

        protected virtual void TickPhysics()
        {
            TickPhysicsInput();
            TickPhysicsTurnDirection();
            
        }

        protected virtual void OnGravityScaleChanged()
        {
            Assert.IsNotNull(_rigidbody, "_rigidbody != null");
            _rigidbody.gravityScale = _gravityScaleInstance.Value;
        }
    }
}