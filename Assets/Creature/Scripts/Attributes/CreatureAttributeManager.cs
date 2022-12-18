using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Creature.Scripts.Attributes
{
    public class CreatureAttributeManager : MonoBehaviour
    {
        public delegate void AttributeValueChangedDelegate(CreatureAttributeInstance attribute);
        
        private readonly Dictionary<string, CreatureAttributeInstance> _attributes = new();
        public event AttributeValueChangedDelegate AttributeValueChanged;

        public CreatureAttributeInstance GetAttribute(CreatureAttribute attribute)
        {
            return _attributes[attribute.Name];
        }
        
        public CreatureAttributeInstance GetAttributeChecked(CreatureAttribute attribute)
        {
            Assert.IsTrue(_attributes.ContainsKey(attribute.Name), "_attributes.ContainsKey(attribute.Name)");
            return _attributes[attribute.Name];
        }

        public float GetAttributeValue(CreatureAttribute attribute)
        {
            return GetAttributeChecked(attribute).Value;
        }
        
        public CreatureAttributeInstance RegisterAttribute(CreatureAttribute attribute, float baseValue)
        {
            if (attribute == null)
            {
                Debug.LogErrorFormat(this, "Attempt to register null attribute");
                return null;
            }
            if (_attributes.ContainsKey(attribute.Name))
            {
                Debug.LogErrorFormat(this, "Attempt to register Duplicate Attribute {0}", attribute.Name);
                return _attributes[attribute.Name];
            }
            CreatureAttributeInstance attributeInstance = new CreatureAttributeInstance(attribute, baseValue);
            
            _attributes.Add(attribute.Name, attributeInstance);
            attributeInstance.ValueChanged += () => OnAttributeValueChanged(attributeInstance);
            return attributeInstance;
        }

        public CreatureAttributeInstance RegisterAttribute(CreatureAttribute attribute)
        {
            if (attribute == null)
            {
                Debug.LogErrorFormat(this, "Attempt to register null attribute");
                return null;
            }
            return RegisterAttribute(attribute, attribute.BaseValue);
        }

        private void OnAttributeValueChanged(CreatureAttributeInstance attributeInstance)
        {
            AttributeValueChanged?.Invoke(attributeInstance);
        }
    }
}