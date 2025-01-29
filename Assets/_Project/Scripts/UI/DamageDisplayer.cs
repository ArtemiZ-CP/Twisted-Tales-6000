using TMPro;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Quantum.Game
{
    public class DamageDisplayer : MonoBehaviour
    {
        [SerializeField] private GameObject _damageArea;
        [SerializeField] private Transform _context;
        [SerializeField] private TMP_Text _damageTextPrefab;

        private readonly List<TMP_Text> _activeTexts = new();

        private void Awake()
        {
            _damageArea.SetActive(false);

            QuantumEvent.Subscribe<EventDisplayStats>(listener: this, handler: DisplayStats);
            QuantumEvent.Subscribe<EventEndRound>(listener: this, handler: HideDamageArea);
        }

        private void HideDamageArea(EventEndRound eventEndRound)
        {
            ClearActiveTexts();
            _damageArea.SetActive(false);
        }

        private void DisplayStats(EventDisplayStats eventDisplayDamage)
        {
            if (QuantumConnection.IsPlayerMe(eventDisplayDamage.Player1)
                || QuantumConnection.IsPlayerMe(eventDisplayDamage.Player2))
            {
                _damageArea.SetActive(true);

                ClearActiveTexts();

                var team1Heroes = eventDisplayDamage.Heroes
                    .Where(h => h.TeamNumber == GameplayConstants.Team1)
                    .OrderByDescending(h => h.DealedDamage)
                    .ToList();

                var team2Heroes = eventDisplayDamage.Heroes
                    .Where(h => h.TeamNumber == GameplayConstants.Team2)
                    .OrderByDescending(h => h.DealedDamage)
                    .ToList();

                AddText("Team 1:");
                foreach (FightingHero hero in team1Heroes)
                {
                    string heroName = QuantumConnection.GetHeroInfo(hero.Hero.ID).Name;
                    AddText($"{heroName}: {hero.DealedDamage} damage");
                }

                AddText("Team 2:");
                foreach (FightingHero hero in team2Heroes)
                {
                    string heroName = QuantumConnection.GetHeroInfo(hero.Hero.ID).Name;
                    AddText($"{heroName}: {hero.DealedDamage} damage");
                }
            }
        }

        private void AddText(string text)
        {
            foreach (var activeText in _activeTexts)
            {
                if (activeText.gameObject.activeInHierarchy == false)
                {
                    activeText.gameObject.SetActive(true);
                    activeText.text = text;
                    return;
                }
            }

            var damageText = Instantiate(_damageTextPrefab, _context);
            _activeTexts.Add(damageText);
            damageText.text = text;
        }

        private void ClearActiveTexts()
        {
            for (int i = 0; i < _activeTexts.Count; i++)
            {
                _activeTexts[i].gameObject.SetActive(false);
            }
        }
    }
}