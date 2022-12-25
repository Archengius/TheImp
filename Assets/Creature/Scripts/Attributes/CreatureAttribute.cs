namespace Creature.Scripts.Attributes
{
    public sealed class CreatureAttribute
    {
        public readonly string Name;
        public readonly float BaseValue;

        public CreatureAttribute(string name, float baseValue)
        {
            Name = name;
            BaseValue = baseValue;
        }

        private bool Equals(CreatureAttribute other)
        {
            return Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is CreatureAttribute attribute && Equals(attribute);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }
    }
}