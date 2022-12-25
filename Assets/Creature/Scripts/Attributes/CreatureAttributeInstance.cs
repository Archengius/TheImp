using System;
using System.Collections.Generic;
using System.Linq;

namespace Creature.Scripts.Attributes
{
    public sealed class CreatureAttributeInstance
    {
        public delegate void AttributeValueChangedDelegate();
        
        public readonly CreatureAttribute Attribute;
        public readonly float BaseValue;
        public float Value { get; private set; }
        public event AttributeValueChangedDelegate ValueChanged;
        public List<CreatureAttributeModifier> Modifiers => _modifiers.Values.ToList();

        private readonly Dictionary<Guid, CreatureAttributeModifier> _modifiers = new();

        public CreatureAttributeInstance(CreatureAttribute attribute, float baseValue)
        {
            Attribute = attribute;
            BaseValue = baseValue;
            RecomputeAttributeValue();
        }

        public void AddModifier(CreatureAttributeModifier modifier)
        {
            _modifiers.Add(modifier.Id, modifier);
            RecomputeAttributeValue();
        }

        public void RemoveModifier(CreatureAttributeModifier modifier)
        {
            RemoveModifier(modifier.Id);
        }

        public void RemoveModifier(Guid modifierId)
        {
            if (_modifiers.Remove(modifierId))
            {
                RecomputeAttributeValue();
            }
        }

        private void RecomputeAttributeValue()
        {
            float modifierAddedValue = 0.0f;
            float baseValueMultiplier = 1.0f;
            float finalValueMultiplier = 1.0f;

            float overwrittenFinalValue = 0.0f;
            bool hasOverwrittenFinalValue = false;

            var sortedModifiers = _modifiers.Values.ToList();
            sortedModifiers.Sort((x, y) => y.Priority - x.Priority);
            
            foreach (var modifier in sortedModifiers)
            {
                switch (modifier.Operation)
                {
                    case AttributeModifierOperation.Add:
                        modifierAddedValue += modifier.Value;
                        break;
                    case AttributeModifierOperation.MultiplyBase:
                        baseValueMultiplier *= modifier.Value;
                        break;
                    case AttributeModifierOperation.MultiplyTotal:
                        finalValueMultiplier *= modifier.Value;
                        break;
                    case AttributeModifierOperation.Overwrite:
                        overwrittenFinalValue = modifier.Value;
                        hasOverwrittenFinalValue = true;
                        break;
                }
            }
            var resultValue = hasOverwrittenFinalValue
                ? overwrittenFinalValue
                : finalValueMultiplier * (baseValueMultiplier * BaseValue + modifierAddedValue);
            SetCurrentValue(resultValue);
        }

        private void SetCurrentValue(float newValue)
        {
            if (!(Math.Abs(Value - newValue) > 0.001f)) return;
            Value = newValue;
            ValueChanged?.Invoke();
        }
    }
}