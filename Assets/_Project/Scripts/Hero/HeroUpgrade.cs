using Quantum.Game;
using UnityEngine;

public class HeroUpgrade : MonoBehaviour
{
    [SerializeField] private UpgradeButton _upgradeButton;

    private HeroMesh _heroMesh;
    private int _id = -1;
    private int _level = -1;

    private void Awake()
    {
        FindHeroMesh();
    }

    private void OnEnable()
    {
        if (_heroMesh.ID >= 0)
        {
            SetHero(_heroMesh.ID, _heroMesh.Level);
        }

        _heroMesh.OnSetHero += SetHero;
        HeroesUpgrade.OnAdd += ShowButton;
        HeroesUpgrade.OnRemove += HideButton;
    }

    private void OnDisable()
    {
        _heroMesh.OnSetHero -= SetHero;
        HeroesUpgrade.OnAdd -= ShowButton;
        HeroesUpgrade.OnRemove -= HideButton;
    }

    private void SetHero(int id, int level)
    {
        _id = id;
        _level = level;

        if (HeroesUpgrade.ContainsUpgrade(id, level))
        {
            _upgradeButton.ShowButton(id, level);
        }
        else
        {
            _upgradeButton.HideButton();
        }
    }

    private void ShowButton(int id, int level)
    {
        if (id == _id && level == _level)
        {
            _upgradeButton.ShowButton(id, level);
        }
    }

    private void HideButton(int id, int level)
    {
        if (id == _id && level == _level)
        {
            _upgradeButton.HideButton();
        }
    }

    private void FindHeroMesh()
    {
        Transform parentTransform = transform;

        while (parentTransform.parent.TryGetComponent(out _heroMesh) == false)
        {
            parentTransform = parentTransform.parent;

            if (parentTransform == null)
            {
                return;
            }
        }
    }
}
