using System.Collections.Generic;
using UnityEngine;

namespace Character
{
    public class CharController : MonoBehaviour
    {
        /** Anim controller used when no other higher priority anim controller is available */
        [SerializeField] private RuntimeAnimatorController primaryAnimController;
        
    }
}