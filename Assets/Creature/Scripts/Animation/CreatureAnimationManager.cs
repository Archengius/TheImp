using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Creature.Scripts.Animation
{
    internal class AnimatorInstanceHandle : IAnimatorInstance
    {
        private Animator _animator;
        private Dictionary<string, AnimatorControllerParameterType> _parameters = new();

        public AnimatorInstanceHandle(Animator animator)
        {
            _animator = animator;
            CacheAllParameters();
        }

        public void CacheAllParameters()
        {
            _parameters.Clear();
            foreach (var parameterInfo in _animator.parameters)
            {
                _parameters.Add(parameterInfo.name, parameterInfo.type);
            }
        }

        public bool HasParameterOfType(string name, AnimatorControllerParameterType type)
        {
            return _parameters.ContainsKey(name) && _parameters[name] == type;
        }

        public void SetTrigger(string name)
        {
            if (HasParameterOfType(name, AnimatorControllerParameterType.Trigger))
            {
                _animator.SetTrigger(name);
            }
        }

        public void SetBool(string name, bool value)
        {
            if (HasParameterOfType(name, AnimatorControllerParameterType.Bool))
            {
                _animator.SetBool(name, value);
            }
        }

        public void SetInteger(string name, int value)
        {
            if (HasParameterOfType(name, AnimatorControllerParameterType.Int))
            {
                _animator.SetInteger(name, value);
            }
        }

        public void SetFloat(string name, float value)
        {
            if (HasParameterOfType(name, AnimatorControllerParameterType.Float))
            {
                _animator.SetFloat(name, value);
            }
        }
    }
    
    [RequireComponent(typeof(Animator))]
    public class CreatureAnimationManager : MonoBehaviour
    {
        [SerializeField] private RuntimeAnimatorController defaultAnimatorController;

        private Animator _animator;
        private AnimatorInstanceHandle _animatorInstanceHandle;
        private readonly List<IAnimationControllerCallback> _callbacks = new();

        protected virtual void Start()
        {
            _animator = GetComponent<Animator>();
            _animatorInstanceHandle = new AnimatorInstanceHandle(_animator);
            //Find callbacks among components and register them
            foreach (var animationComponentCallback in GetComponents<IAnimationControllerCallback>())
            {
                RegisterCallback(animationComponentCallback);
            }
        }

        public void RegisterCallback(IAnimationControllerCallback callback)
        {
            _callbacks.Add(callback);
        }

        public void UnregisterCallback(IAnimationControllerCallback callback)
        {
            _callbacks.Remove(callback);
        }

        protected virtual RuntimeAnimatorController PickActiveAnimationController()
        {
            var controller = _callbacks.Select(x => x.GetAnimationControllerOverride())
                .OrderByDescending(x => x.Priority)
                .Select(x => x.Controller)
                .FirstOrDefault();
            
            if (controller == null)
            {
                controller = defaultAnimatorController;
            }
            return controller;
        }

        protected virtual void UpdateAnimatorParams()
        {
            _callbacks.ForEach(x => x.UpdateAnimatorParameters(_animatorInstanceHandle));
        }

        protected virtual void OnAnimationControllerChanged()
        {
            var newController = _animator.runtimeAnimatorController;
            _callbacks.ForEach(x => x.OnAnimationControllerChanged(newController));
        }

        protected virtual void Update()
        {
            if (!_animator) return;
            RuntimeAnimatorController activeController = PickActiveAnimationController();
            if (_animator.runtimeAnimatorController != activeController)
            {
                _animator.runtimeAnimatorController = activeController;
                _animatorInstanceHandle.CacheAllParameters();
                OnAnimationControllerChanged();
            }
            UpdateAnimatorParams();
        }

        public void TriggerAnimNotify(AnimationEvent animationEvent)
        {
            var notifyName = animationEvent.stringParameter;
            var customData = animationEvent.intParameter;
            _callbacks.ForEach(x => x.OnAnimNotifyTriggered(notifyName, customData));
        }
    }
}