using System.Collections.Generic;
using System.Linq;
using Photon.Deterministic;
using TMPro;
using UnityEngine;

namespace Quantum.Game
{
    public abstract class StatDisplayer : MonoBehaviour
    {
        [SerializeField] private Transform _context;
        [SerializeField] private TMP_Text _textPrefab;

        private readonly List<TMP_Text> _texts = new();

        private void Awake()
        {
            QuantumEvent.Subscribe<EventDisplayStats>(listener: this, handler: DisplayStats);
            QuantumEvent.Subscribe<EventEndRound>(listener: this, handler: HideDamageArea);
        }

        protected abstract void GetDisplaySettings(out string header, out System.Func<FightingHero, FP> statSelector);

        private void HideDamageArea(EventEndRound eventEndRound)
        {
            ClearActiveTexts();
        }

        private void DisplayStats(EventDisplayStats eventDisplayDamage)
        {
            if (QuantumConnection.IsPlayerMe(eventDisplayDamage.Player1)
                || QuantumConnection.IsPlayerMe(eventDisplayDamage.Player2))
            {
                GetDisplaySettings(out string header, out System.Func<FightingHero, FP> statSelector);
                DisplayHeroStats(eventDisplayDamage, header, statSelector);
            }
        }

        private void DisplayHeroStats(EventDisplayStats eventDisplayDamage, string header, System.Func<FightingHero, FP> statSelector)
        {
            ClearActiveTexts();

            var team1Heroes = eventDisplayDamage.Heroes
                .Where(h => h.Hero.ID >= 0 && h.TeamNumber == GameplayConstants.Team1)
                .OrderByDescending(statSelector)
                .ToList();

            var team2Heroes = eventDisplayDamage.Heroes
                .Where(h => h.Hero.ID >= 0 && h.TeamNumber == GameplayConstants.Team2)
                .OrderByDescending(statSelector)
                .ToList();

            AddText(header);
            AddText("Team 1:");
            foreach (FightingHero hero in team1Heroes)
            {
                string heroName = QuantumConnection.GetHeroInfo(hero.Hero.ID).Name;
                AddText($"{heroName}: {statSelector(hero)}");
            }

            AddText("Team 2:");
            foreach (FightingHero hero in team2Heroes)
            {
                string heroName = QuantumConnection.GetHeroInfo(hero.Hero.ID).Name;
                AddText($"{heroName}: {statSelector(hero)}");
            }
        }

        private void AddText(string text)
        {
            for (int i = 0; i < _texts.Count; i++)
            {
                if (!_texts[i].gameObject.activeSelf)
                {
                    _texts[i].gameObject.SetActive(true);
                    _texts[i].text = text;
                    return;
                }
            }

            var damageText = Instantiate(_textPrefab, _context);
            _texts.Add(damageText);
            damageText.text = text;
        }

        private void ClearActiveTexts()
        {
            for (int i = 0; i < _texts.Count; i++)
            {
                _texts[i].gameObject.SetActive(false);
            }
        }
    }
}