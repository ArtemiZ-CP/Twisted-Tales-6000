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
        }

        protected abstract void GetDisplaySettings(out string header, out System.Func<FightingHero, FP> statSelector);

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
            AddText(header, TextStyle.Title);

            var (myTeam, enemyTeam) = QuantumConnection.IsPlayerMe(eventDisplayDamage.Player1) 
                ? (GameplayConstants.Team1, GameplayConstants.Team2) 
                : (GameplayConstants.Team2, GameplayConstants.Team1);

            DisplayTeamInfo("<color=#00FF00>Me</color>", eventDisplayDamage, myTeam, statSelector);
            DisplayTeamInfo("<color=#FF0000>Enemy</color>", eventDisplayDamage, enemyTeam, statSelector);
        }

        private void DisplayTeamInfo(string title, EventDisplayStats eventDisplayDamage, int team, System.Func<FightingHero, FP> statSelector)
        {
            var fightingHeroes = eventDisplayDamage.Heroes
                .Where(h => h.Hero.ID >= 0 && h.TeamNumber == team)
                .OrderByDescending(statSelector)
                .ToList();

            AddText(title, TextStyle.Title);
            
            foreach (FightingHero hero in fightingHeroes)
            {
                AddText($"{HeroName.GetHeroName(hero.Hero.ID, hero.Hero.Level)}: {statSelector(hero)}");
            }
        }

        private void AddText(string text)
        {
            AddText(text, TextStyle.Default);
        }

        private void AddText(string text, TextStyle style)
        {
            for (int i = 0; i < _texts.Count; i++)
            {
                if (_texts[i].gameObject.activeSelf == false)
                {
                    ConfigureText(_texts[i], text, style);
                    return;
                }
            }

            var damageText = Instantiate(_textPrefab, _context);
            _texts.Add(damageText);
            ConfigureText(damageText, text, style);
        }

        private void ConfigureText(TMP_Text textComponent, string text, TextStyle style)
        {
            textComponent.gameObject.SetActive(true);
            textComponent.text = text;
            textComponent.fontSize = style.FontSize;
            textComponent.fontWeight = style.FontWeight;
            textComponent.alignment = style.Alignment;
        }

        private void ClearActiveTexts()
        {
            for (int i = 0; i < _texts.Count; i++)
            {
                _texts[i].gameObject.SetActive(false);
            }
        }

        public struct TextStyle
        {
            private float _fontSize;
            private FontWeight _fontWeight;
            private TextAlignmentOptions _alignment;

            public readonly float FontSize => _fontSize;
            public readonly FontWeight FontWeight => _fontWeight;
            public readonly TextAlignmentOptions Alignment => _alignment;

            public static TextStyle Default => new()
            {
                _fontSize = 20,
                _fontWeight = FontWeight.Regular,
                _alignment = TextAlignmentOptions.Left
            };

            public static TextStyle Title => new()
            {
                _fontSize = 30,
                _fontWeight = FontWeight.Bold,
                _alignment = TextAlignmentOptions.Center
            };
        }
    }
}