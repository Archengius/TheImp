using System;
using Creature.Scripts.Attributes;
using Creature.Scripts.Movement;
using UnityEngine;
using UnityEngine.Assertions;
using Object = System.Object;

namespace Character
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CreatureAttributeManager))]
    public class CharacterMovementComponent : CreatureMovementComponent
    {
    }
}