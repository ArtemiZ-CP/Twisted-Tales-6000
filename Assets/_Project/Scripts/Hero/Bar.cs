using Photon.Deterministic;
using Quantum;
using UnityEngine;
using UnityEngine.UI;

public abstract class Bar : MonoBehaviour
{
    [SerializeField] private Slider _slider;

    private QuantumEntityView _quantumEntityView;
    private QuantumEntityViewUpdater _quantumEntityViewUpdater;
    private RectTransform _sliderRectTransform;
    private float _maxX;

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
        _sliderRectTransform = _slider.GetComponent<RectTransform>();
        _maxX = _sliderRectTransform.rect.width;
    }

    protected virtual void OnEnable()
    {
        _slider.gameObject.SetActive(false);
    }

    protected void UpdateBar(EntityRef heroEntity, FP amount, FP maxAmount, float progress = 0)
    {
        if (QuantumEntityViewUpdater == null)
        {
            return;
        }

        QuantumEntityView quantumEntityView = QuantumEntityViewUpdater.GetView(heroEntity);

        if (_quantumEntityView == quantumEntityView)
        {
            _slider.gameObject.SetActive(true);
            _slider.value = (amount / maxAmount).AsFloat;
            _sliderRectTransform.anchoredPosition = new Vector2(
                Mathf.Lerp(0, _maxX, progress),
                _sliderRectTransform.anchoredPosition.y
            ); 
        }
    }

    private QuantumEntityView FindFirstEntityViewInParents()
    {
        Transform parent = transform.parent;

        while (parent != null)
        {
            if (parent.TryGetComponent<QuantumEntityView>(out var quantumEntityView))
            {
                return quantumEntityView;
            }

            parent = parent.parent;
        }

        return null;
    }
}
