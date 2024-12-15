using Quantum;
using Quantum.Game;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private QuantumEntityView _quantumEntityView;
    [SerializeField] private Slider _healthBar;

    private QuantumEntityViewUpdater _quantumEntityViewUpdater;

    private QuantumEntityViewUpdater QuantumEntityViewUpdater
    {
        get
        {
            if (_quantumEntityViewUpdater == null)
            {
                _quantumEntityViewUpdater = FindFirstObjectByType<QuantumEntityViewUpdater>();
            }

            return _quantumEntityViewUpdater;
        }
    }

    private void Awake()
    {
        _healthBar.gameObject.SetActive(false);
        QuantumEvent.Subscribe<EventHeroHealthChanged>(listener: this, handler: OnHeroHealthChanged);
    }

    private void OnHeroHealthChanged(EventHeroHealthChanged eventHeroHealthChanged)
    {
        if (QuantumConnection.IsPlayerMe(eventHeroHealthChanged.PlayerRef1) ||
            QuantumConnection.IsPlayerMe(eventHeroHealthChanged.PlayerRef2))
        {
            UpdateHealthBar(eventHeroHealthChanged);
        }
    }

    private void UpdateHealthBar(EventHeroHealthChanged eventHeroHealthChanged)
    {
        if (QuantumEntityViewUpdater == null)
        {
            return;
        }

        QuantumEntityView quantumEntityView = QuantumEntityViewUpdater.GetView(eventHeroHealthChanged.HeroEntity);

        if (_quantumEntityView == quantumEntityView)
        {
            _healthBar.gameObject.SetActive(true);
            float health = (eventHeroHealthChanged.CurrentHealth / eventHeroHealthChanged.MaxHealth).AsFloat;
            _healthBar.value = health;
        }
    }
}
