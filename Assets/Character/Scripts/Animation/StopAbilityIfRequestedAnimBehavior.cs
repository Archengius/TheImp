using Character.Scripts.Ability;
using UnityEngine;

namespace Character.Animation
{
    public class StopAbilityIfRequestedAnimBehavior : StateMachineBehaviour
    {
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            CharacterAbilityManager abilityManager = animator.GetComponent<CharacterAbilityManager>();

            if (abilityManager && abilityManager.ActiveAbility is TimedCharacterAbility timedCharacterAbility)
            {
                if (timedCharacterAbility.RequestedAbilityStop)
                {
                    timedCharacterAbility.SetTimeToAbilityExit();
                }
            }
        }
    }
}