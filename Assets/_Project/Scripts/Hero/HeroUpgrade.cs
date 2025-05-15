using System.Collections.Generic;
using Quantum;
using Quantum.Game;
using UnityEngine;

public class HeroUpgrade : MonoBehaviour
{
    private static readonly List<(int, int)> ActiveUpgrades = new();

    [SerializeField] private UpgradeButton _upgradeButton;

    private HeroMesh _heroMesh;
    private int _id = -1;

    private void Awake()
    {
        QuantumEvent.Subscribe<EventLevelUpHero>(listener: this, handler: ShowButton);
        FindHeroMesh();
    }

    private void OnEnable()
    {
        _id = _heroMesh.ID;
        
        if (ActiveUpgrades.Contains((_id, _heroMesh.Level)))
        {
            _upgradeButton.ShowButton(_id, _heroMesh.Level);
        }
    }

    private void ShowButton(EventLevelUpHero evt)
    {
        if (QuantumConnection.IsPlayerMe(evt.PlayerRef) == false)
        {
            return;
        }

        if (evt.HeroID == _id)
        {
            ActiveUpgrades.Add((evt.HeroID, evt.HeroLevel));
            _upgradeButton.ShowButton(evt.HeroID, evt.HeroLevel);
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
