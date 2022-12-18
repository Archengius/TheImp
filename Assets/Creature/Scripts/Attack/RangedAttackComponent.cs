using UnityEngine;

namespace Creature.Scripts.Attack
{
    //TODO
    public class RangedAttackComponent : MonoBehaviour
    {
        [SerializeField] protected GameObject projectileTemplate;
        [SerializeField] protected Collider2D visibilityColliderComponent;
        [SerializeField] protected bool requireLineOfSightOnTarget = true;
    }
}