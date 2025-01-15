using Quantum;
using Quantum.Game;

public class ManaBar : Bar
{
    protected override void Awake()
    {
        base.Awake();
        QuantumEvent.Subscribe<EventHeroManaChanged>(listener: this, handler: OnHeroManaChanged);
    }

    private void OnHeroManaChanged(EventHeroManaChanged eventHeroManaChanged)
    {
        if (QuantumConnection.IsPlayerMe(eventHeroManaChanged.PlayerRef1) ||
            QuantumConnection.IsPlayerMe(eventHeroManaChanged.PlayerRef2))
        {
            UpdateBar(eventHeroManaChanged.HeroEntity, eventHeroManaChanged.CurrentMana, eventHeroManaChanged.MaxMana);
        }
    }
}
