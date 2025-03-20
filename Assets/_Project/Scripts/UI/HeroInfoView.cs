using Quantum.Game;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Deterministic;
using Quantum;

public class HeroInfoView : MonoBehaviour
{
    [SerializeField] private HeroesMover _heroesMover;
    [SerializeField] private GameObject _heroInfoPanel;
    [Header("Hero Info")]
    [SerializeField] private TMP_Text _heroName;
    [SerializeField] private TMP_Text _heroStars;
    [SerializeField] private Slider _heroHealthBar;
    [SerializeField] private TMP_Text _heroDamage;
    [SerializeField] private TMP_Text _heroAttackSpeed;
    [SerializeField] private TMP_Text _heroDPS;
    [SerializeField] private TMP_Text _heroDefense;
    [SerializeField] private TMP_Text _heroSellPrice;
    [SerializeField] private UnityEngine.UI.Button _sellButton;

    private HeroObject _selectedHero;

    private void Awake()
    {
        _heroInfoPanel.SetActive(false);
        QuantumEvent.Subscribe<EventGetFightingHero>(listener: this, handler: GetFightingHero);
    }

    private void OnEnable()
    {
        _heroesMover.ClickedOnHero += SelectHero;
        _sellButton.onClick.AddListener(SellHero);
    }

    private void OnDisable()
    {
        _heroesMover.ClickedOnHero -= SelectHero;
        _sellButton.onClick.RemoveListener(SellHero);
    }

    private void FixedUpdate()
    {
        if (_heroesMover.IsRoundStarted && _heroesMover.SelectedHeroRef != default)
        {
            if (QuantumConnection.IsAbleToConnectQuantum())
            {
                CommandGetHeroInfo commandGetHeroInfo = new()
                {
                    EntityRef = _heroesMover.SelectedHeroRef
                };

                QuantumRunner.DefaultGame.SendCommand(commandGetHeroInfo);
            }
        }
    }

    private void SellHero()
    {
        _heroesMover.TrySellHero(_selectedHero);
    }

    private void GetFightingHero(EventGetFightingHero eventGetFightingHero)
    {
        if (_heroesMover.SelectedHeroRef == default)
        {
            return;
        }

        DisplayMainStats(eventGetFightingHero.FightingHero);

        _sellButton.gameObject.SetActive(false);
    }

    private void SelectHero(HeroObject hero)
    {
        if (_heroesMover.SelectedHeroRef != default)
        {
            return;
        }


        if (hero == null || hero.State == HeroState.Shop)
        {
            _selectedHero = null;
            _heroInfoPanel.SetActive(false);
            return;
        }

        _selectedHero = hero;
        _heroInfoPanel.SetActive(true);
        _sellButton.gameObject.SetActive(true);

        int level = hero.Level;

        DisplayMainStats(hero.Id, level);
    }

    private void DisplayMainStats(FightingHero fightingHero)
    {
        _heroInfoPanel.SetActive(true);
        HeroInfo heroInfo = QuantumConnection.GetHeroInfo(fightingHero.Hero.ID);
        DisplayMainStats(heroInfo, fightingHero.Hero.Level, fightingHero.CurrentHealth);
    }

    private void DisplayMainStats(int id, int level)
    {
        HeroInfo heroInfo = QuantumConnection.GetHeroInfo(id);
        DisplayMainStats(heroInfo, level, heroInfo.HeroStats[level].Health);
    }

    private void DisplayMainStats(HeroInfo heroInfo, int level, FP currentHealth)
    {
        HeroLevelStats heroStats = heroInfo.HeroStats[level];

        _heroName.text = heroInfo.Name.ToString();

        string stars = "";

        for (int i = 0; i < level + 1; i++)
        {
            stars += "<sprite index=0>";
        }

        _heroStars.text = stars;
        _heroHealthBar.value = (float)(currentHealth / heroStats.Health);
        _heroDamage.text = heroStats.AttackDamage.ToString("0.##");
        _heroAttackSpeed.text = heroStats.AttackSpeed.ToString("0.##");
        _heroDPS.text = (heroStats.AttackDamage * heroStats.AttackSpeed).ToString("0.##");
        _heroDefense.text = heroStats.Defense.ToString("0.##");
        _heroSellPrice.text = $"SELL\n{QuantumConnection.GameConfig.GetHeroSellCost(heroInfo.Rare, level):0.##} coins";
    }
}
