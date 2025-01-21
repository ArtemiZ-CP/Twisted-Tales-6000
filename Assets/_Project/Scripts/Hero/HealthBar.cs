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
            UpdateBar(eventHeroHealthChanged.HeroEntity, eventHeroHealthChanged.CurrentHealth, eventHeroHealthChanged.MaxHealth);
        }
    }
}
