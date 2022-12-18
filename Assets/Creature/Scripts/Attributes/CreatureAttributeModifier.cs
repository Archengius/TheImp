using System;

namespace Creature.Scripts.Attributes
{
    public enum AttributeModifierPriority
    {
        Low = -100,
        Normal = 0,
        GameplayLevel = 1,
        ActiveAbility = 10,
        High = 100
    }
    
    public enum AttributeModifierOperation
    {
        Add,
        MultiplyBase,
        MultiplyTotal,
        Overwrite
    }
    
    public sealed class CreatureAttributeModifier
    {
        public readonly Guid Id;
        public readonly string Description;
        public readonly int Priority;
        public readonly AttributeModifierOperation Operation;
        public readonly float Value;

        public CreatureAttributeModifier(Guid id, string Description, int priority, AttributeModifierOperation operation, float value)
        {
            Id = id;
            Description = this.Description;
            Priority = priority;
            Operation = operation;
            Value = value;
        }
    }
}