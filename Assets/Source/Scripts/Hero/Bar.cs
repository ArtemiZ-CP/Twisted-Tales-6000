using Photon.Deterministic;
using Quantum;
using UnityEngine;
using UnityEngine.UI;

public abstract class Bar : MonoBehaviour
{
    [SerializeField] private Slider _slider;

    private QuantumEntityView _quantumEntityView;
    private QuantumEntityViewUpdater _quantumEntityViewUpdater;

    protected Slider Slider => _slider;
    protected QuantumEntityViewUpdater QuantumEntityViewUpdater
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
    
    protected virtual void Awake()
    {
        _quantumEntityView = FindFirstEntityViewInParents();
    }

    protected virtual void OnEnable()
    {
        _slider.gameObject.SetActive(false);
    }

    protected void UpdateBar(EntityRef heroEntity, FP currentHealth, FP maxHealth)
    {
        if (QuantumEntityViewUpdater == null)
        {
            return;
        }

        QuantumEntityView quantumEntityView = QuantumEntityViewUpdater.GetView(heroEntity);

        if (_quantumEntityView == quantumEntityView)
        {
            Slider.gameObject.SetActive(true);
            float health = (currentHealth / maxHealth).AsFloat;
            Slider.value = health;
        }
    }

    private QuantumEntityView FindFirstEntityViewInParents()
    {
        Transform parent = transform.parent;

        while (parent != null)
        {
            QuantumEntityView quantumEntityView = parent.GetComponent<QuantumEntityView>();

            if (quantumEntityView != null)
            {
                return quantumEntityView;
            }

            parent = parent.parent;
        }

        return null;
    }
}
