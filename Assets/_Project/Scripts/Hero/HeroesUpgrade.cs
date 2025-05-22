using System.Collections.Generic;
using Quantum;
using Quantum.Game;
using UnityEngine;

public class HeroesUpgrade : MonoBehaviour
{
    private static readonly List<(int, int)> ActiveUpgrades = new();

    public static event System.Action<int, int> OnAdd;
    public static event System.Action<int, int> OnRemove;

    private void Awake()
    {
        QuantumEvent.Subscribe<EventLevelUpHero>(listener: this, handler: LevelUpHero);
    }

    public static bool ContainsUpgrade(int id, int level)
    {
        return ActiveUpgrades.Contains((id, level));
    }

    private void LevelUpHero(EventLevelUpHero evt)
    {
        if (QuantumConnection.IsPlayerMe(evt.PlayerRef) == false)
        {
            return;
        }

        if (evt.IsCompleted)
        {
            if (ActiveUpgrades.Contains((evt.HeroID, evt.HeroLevel)))
            {
                ActiveUpgrades.Remove((evt.HeroID, evt.HeroLevel));
                OnRemove?.Invoke(evt.HeroID, evt.HeroLevel);
            }
        }
        else
        {
            if (ActiveUpgrades.Contains((evt.HeroID, evt.HeroLevel)) == false)
            {
                ActiveUpgrades.Add((evt.HeroID, evt.HeroLevel));
                OnAdd?.Invoke(evt.HeroID, evt.HeroLevel);
            }
        }
    }
}
