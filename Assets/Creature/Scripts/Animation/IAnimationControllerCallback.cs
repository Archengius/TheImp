using UnityEngine;

namespace Creature.Scripts.Animation
{
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
    
    public interface IAnimationControllerCallback
    {
        public virtual AnimationControllerOverride GetAnimationControllerOverride()
        {
            return new AnimationControllerOverride(null);
        }

        public virtual void UpdateAnimatorParameters(Animator animator)
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