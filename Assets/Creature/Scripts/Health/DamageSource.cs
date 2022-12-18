using System;
using UnityEngine;

namespace Creature.Scripts.Health
{
    public class DamageSource
    {
        public string Name;
        public readonly GameObject Source;
        public readonly GameObject Attacker;
        public bool CanBeBlocked;

        protected DamageSource(string name, GameObject source, GameObject attacker, bool canBeBlocked)
        {
            Name = name;
            Source = source;
            Attacker = attacker;
            CanBeBlocked = canBeBlocked;
        }

        public static DamageSource CauseMeleeDamage(MonoBehaviour source, bool canBeBlocked = true)
        {
            var gameObject = source.gameObject;
            return new DamageSource("melee", gameObject, gameObject, canBeBlocked);
        } 
    }
}