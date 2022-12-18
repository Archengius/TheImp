namespace Creature.Scripts.Health
{
    public interface IHealthComponentCallback
    {
        virtual void OnRegistered(CreatureHealthComponent healthComponent)
        {
        }
        
        virtual void OnHealthChanged(int newHealth)
        {
        }

        virtual void OnMaxHealthChanged(int newMaxHealth)
        {
        }

        virtual void OnDamageTaken(DamageSource damageSource, int damageAmount)
        {
        }

        virtual void OnRanOutOfHealth()
        {
        }

        virtual bool AdjustDamage(DamageSource damageSource, ref int damageAmount)
        {
            return false;
        }
    }
}