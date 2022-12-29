using System;
using System.Collections.Generic;
using System.Linq;
using Creature.Scripts.Animation;
using Creature.Scripts.Attributes;
using Creature.Scripts.Movement;
using UnityEngine;

namespace Creature.Scripts.Health
{
    [RequireComponent(typeof(CreatureAttributeManager))]
    public class CreatureHealthComponent : DamageReceiverComponent, IAnimationControllerCallback
    {
        public static readonly CreatureAttribute MaxHealthAttribute = new("health.max_health", 6.0f);
        
        private CreatureAttributeManager _attributeManager;
        private CreatureMovementComponent _movementComponent;
        private CreatureAttributeInstance _maxHealthInstance;

        [SerializeField] private int baseMaxHealth = 6;
        [SerializeField] private float baseKnockBackStrength = 0.0f;
        [SerializeField] private RuntimeAnimatorController deathAnimationController;

        private int _currentHealth;
        private bool _hasRanOutOfHealth;
        private readonly List<IHealthComponentCallback> _callbacks = new();

        public int MaxHealth => (int)_maxHealthInstance.Value;
        public int Health => _currentHealth;
        public bool RanOutOfHealth => _hasRanOutOfHealth;

        public AnimationControllerOverride GetAnimationControllerOverride()
        {
            if (RanOutOfHealth && deathAnimationController)
            {
                return new AnimationControllerOverride(deathAnimationController,
                    AnimControllerOverridePriority.DeathAnimation);
            }

            return new AnimationControllerOverride(null);
        }

        public void UpdateAnimatorParameters(IAnimatorInstance animator)
        {
            animator.SetFloat("healthPct", Health / Math.Max(0.1f, MaxHealth));
        }

        protected virtual void Start()
        {
            _attributeManager = GetComponent<CreatureAttributeManager>();
            _movementComponent = GetComponent<CreatureMovementComponent>();
            if (_attributeManager == null)
            {
                Debug.LogErrorFormat(this, "Failed to find CreatureAttributeManager on GameObject");
                return;
            }
            _maxHealthInstance = _attributeManager.RegisterAttribute(MaxHealthAttribute, baseMaxHealth);
            if (_maxHealthInstance != null)
            {
                _maxHealthInstance.ValueChanged += OnMaxHealthChanged;
                _currentHealth = MaxHealth;
            }
            
            //Find callbacks among components and register them
            foreach (var healthComponentCallback in GetComponents<IHealthComponentCallback>())
            {
                RegisterCallback(healthComponentCallback);
            }
        }

        protected virtual void Update()
        {
        }

        public void RegisterCallback(IHealthComponentCallback callback)
        {
            if (_callbacks.Contains(callback))
            {
                Debug.LogErrorFormat(this, "Attempt to register duplicate callback {0}", callback);
                return;
            }
            _callbacks.Add(callback);
            callback.OnRegistered(this);
        }

        public void RemoveCallback(IHealthComponentCallback callback)
        {
            _callbacks.Remove(callback);
        }

        public virtual void Heal(int health)
        {
            if (health <= 0)
            {
                Debug.LogErrorFormat(this, "Heal called with negative health");
                return;
            }
            _hasRanOutOfHealth = false;
            SetHealth(Health + health);
        }

        public override bool CanTakeDamage(DamageSource source)
        {
            return true;
        }

        protected virtual void ApplyKnockBack(GameObject damageSource)
        {
            if (damageSource && damageSource.transform && _movementComponent && baseKnockBackStrength != 0.0f)
            {
                Vector3 knockBackDir = (transform.position - damageSource.transform.position).normalized;
                _movementComponent.AddImpulse(knockBackDir * baseKnockBackStrength);
            }
        }

        public override bool AttackFrom(DamageSource source, int damageAmount)
        {
            if (!CanTakeDamage(source))
            {
                return false;
            }
            int resultDamage = Mathf.Min(Health, damageAmount);
            _callbacks.ForEach(x => x.AdjustDamage(source, ref resultDamage));

            if (resultDamage <= 0)
            {
                return false;
            }
            SetHealth(Health - resultDamage);
            if (source.Source && source.Source.transform)
            {
                ApplyKnockBack(source.Source);
            } 
            
            OnDamageTaken(source, resultDamage);
            return true;
        }

        protected virtual void SetHealth(int newHealth)
        {
            int newHealthClamped = Mathf.Clamp(newHealth, 0, MaxHealth);
            if (_currentHealth != newHealthClamped)
            {
                _currentHealth = newHealthClamped;
                OnHealthChanged();
            }
            if (_currentHealth == 0 && !_hasRanOutOfHealth)
            {
                _hasRanOutOfHealth = true;
                OnRanOutOfHealth();
            }
        }

        protected virtual void OnDamageTaken(DamageSource damageSource, int amount)
        {
            _callbacks.ForEach(x => x.OnDamageTaken(damageSource, amount));
        }

        protected virtual void OnMaxHealthChanged()
        {
            SetHealth(Health);
            _callbacks.ForEach(x => x.OnMaxHealthChanged(MaxHealth));
        }

        protected virtual void OnHealthChanged()
        {
            _callbacks.ForEach(x => x.OnHealthChanged(_currentHealth));
        }

        protected virtual void OnRanOutOfHealth()
        {
            _callbacks.ForEach(x => x.OnRanOutOfHealth());
        }
    }
}