using UnityEngine;

namespace Creature.Scripts.Animation
{
    public class AnimControllerOverridePriority
    {
        public const int Normal = 0;
        public const int ActiveAbility = 100;
        public const int DeathAnimation = 1000;
    }
    
    public struct AnimationControllerOverride
    {
        public readonly RuntimeAnimatorController Controller;
        public readonly int Priority;

        public AnimationControllerOverride(RuntimeAnimatorController controller, int priority = 0)
        {
            Controller = controller;
            Priority = priority;
        }
    }

    public interface IAnimatorInstance
    {
        void SetTrigger(string name);
        void SetBool(string name, bool value);
        void SetInteger(string name, int value);
        void SetFloat(string name, float value);
    }

    public interface IAnimationControllerCallback
    {
        public virtual AnimationControllerOverride GetAnimationControllerOverride()
        {
            return new AnimationControllerOverride(null);
        }

        public virtual void UpdateAnimatorParameters(IAnimatorInstance animator)
        {
        }

        public virtual void OnAnimationControllerChanged(RuntimeAnimatorController newController)
        {
        }

        public virtual void OnAnimNotifyTriggered(string name, int customData)
        {
        }
    }
}