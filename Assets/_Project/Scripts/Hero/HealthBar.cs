using Photon.Deterministic;
using Quantum;
using Quantum.Game;

public class HealthBar : Bar
{
    protected override void Awake()
    {
        base.Awake();
        QuantumEvent.Subscribe<EventHeroHealthChanged>(listener: this, handler: OnHeroHealthChanged);
    }

    private void OnHeroHealthChanged(EventHeroHealthChanged eventHeroHealthChanged)
    {
        if (QuantumConnection.IsPlayerMe(eventHeroHealthChanged.PlayerRef1) ||
            QuantumConnection.IsPlayerMe(eventHeroHealthChanged.PlayerRef2))
        {
            FP armor = eventHeroHealthChanged.CurrentArmor;
            FP maxHealth = eventHeroHealthChanged.MaxHealth + armor;
            FP currentHealth = eventHeroHealthChanged.CurrentHealth;
            UpdateBar(eventHeroHealthChanged.HeroEntity, currentHealth, maxHealth);
        }
    }
}
