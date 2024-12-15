using Quantum;
using Quantum.Game;
using UnityEngine;
using UnityEngine.UI;

public class ManaBar : MonoBehaviour
{
    [SerializeField] private QuantumEntityView _quantumEntityView;
    [SerializeField] private Slider _manaBar;

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
        _manaBar.gameObject.SetActive(false);
        QuantumEvent.Subscribe<EventHeroManaChanged>(listener: this, handler: OnHeroManaChanged);
    }

    private void OnHeroManaChanged(EventHeroManaChanged eventHeroManaChanged)
    {
        if (QuantumConnection.IsPlayerMe(eventHeroManaChanged.PlayerRef1) ||
            QuantumConnection.IsPlayerMe(eventHeroManaChanged.PlayerRef2))
        {
            UpdateManaBar(eventHeroManaChanged);
        }
    }

    private void UpdateManaBar(EventHeroManaChanged eventHeroManaChanged)
    {
        if (QuantumEntityViewUpdater == null)
        {
            return;
        }

        QuantumEntityView quantumEntityView = QuantumEntityViewUpdater.GetView(eventHeroManaChanged.HeroEntity);

        if (_quantumEntityView == quantumEntityView)
        {
            _manaBar.gameObject.SetActive(true);
            float mana = (eventHeroManaChanged.CurrentMana / eventHeroManaChanged.MaxMana).AsFloat;
            _manaBar.value = mana;
        }
    }
}
