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
    [SerializeField] private GameObject _sellPanel;
    [Header("Hero Info")]
    [SerializeField] private TMP_Text _heroName;
    [SerializeField] private TMP_Text _heroStars;
    [SerializeField] private Slider _heroHealthBar;
    [SerializeField] private Slider _heroArmorBar;
    [SerializeField] private TMP_Text _heroHealth;
    [SerializeField] private TMP_Text _heroDamage;
    [SerializeField] private TMP_Text _heroAttackSpeed;
    [SerializeField] private TMP_Text _heroDPS;
    [SerializeField] private TMP_Text _heroDefense;
    [SerializeField] private TMP_Text[] _heroSellPrice;
    [SerializeField] private UnityEngine.UI.Button _sellButton;

    private HeroObject _selectedHero;
    private bool _isCursorInArea;
    private RectTransform _healthRectTransform;
    private RectTransform _armorRectTransform;

    public bool IsCursorInArea => _isCursorInArea;

    private void Awake()
    {
        _heroInfoPanel.SetActive(false);
        _sellPanel.SetActive(false);
        _healthRectTransform = _heroHealthBar.GetComponent<RectTransform>();
        _armorRectTransform = _heroArmorBar.GetComponent<RectTransform>();
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

    public void PointerEnter()
    {
        _isCursorInArea = true;

        if (_heroesMover.IsHeroDragging)
        {
            _sellPanel.SetActive(true);
        }
    }

    public void PointerExit()
    {
        _isCursorInArea = false;
        _sellPanel.SetActive(false);
    }

    private void SellHero()
    {
        _heroesMover.TrySellHero(_selectedHero);
        _heroInfoPanel.SetActive(false);
        _sellPanel.SetActive(false);
    }

    private void GetFightingHero(EventGetFightingHero eventGetFightingHero)
    {
        if (QuantumConnection.IsPlayerMe(eventGetFightingHero.PlayerRef))
        {
            DisplayMainStats(eventGetFightingHero.FightingHero);
            _sellButton.gameObject.SetActive(false);
        }
    }

    private void SelectHero(HeroObject hero)
    {
        if (_isCursorInArea)
        {
            return;
        }

        if (_heroesMover.IsRoundStarted)
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
        else
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
    }

    private void DisplayMainStats(FightingHero fightingHero)
    {
        if (fightingHero.Hero.Ref == default)
        {
            _heroInfoPanel.SetActive(false);
            return;
        }

        _heroInfoPanel.SetActive(true);
        HeroInfo heroInfo = QuantumConnection.GetHeroInfo(fightingHero.Hero.ID);
        FP armor = fightingHero.CurrentArmor;
        FP maxHealth = fightingHero.Hero.Health + armor;
        FP currentHealth = fightingHero.CurrentHealth;
        DisplayMainStats(heroInfo, fightingHero.Hero.Level, armor, currentHealth, maxHealth);
    }

    private void DisplayMainStats(int id, int level)
    {
        HeroInfo heroInfo = QuantumConnection.GetHeroInfo(id);
        FP health = heroInfo.HeroStats[level].Health;
        DisplayMainStats(heroInfo, level, armor: 0, health, health);
    }

    private void DisplayMainStats(HeroInfo heroInfo, int level, FP armor, FP currentHealth, FP maxHealth)
    {
        HeroLevelStats heroStats = heroInfo.HeroStats[level];

        _heroName.text = heroInfo.Name.ToString();

        string stars = "";

        for (int i = 0; i < level + 1; i++)
        {
            stars += "<sprite index=0>";
        }

        _heroStars.text = stars;
        UpdateBar(_heroHealthBar, _healthRectTransform, currentHealth, maxHealth);
        UpdateBar(_heroArmorBar, _armorRectTransform, armor, maxHealth, (currentHealth / maxHealth).AsFloat);
        _heroHealth.text = $"{(currentHealth + armor).ToString("0.##")}/{heroStats.Health.ToString("0.##")}";
        _heroDamage.text = heroStats.AttackDamage.ToString("0.##");
        _heroAttackSpeed.text = heroStats.AttackSpeed.ToString("0.##");
        _heroDPS.text = (heroStats.AttackDamage * heroStats.AttackSpeed).ToString("0.##");
        _heroDefense.text = heroStats.Defense.ToString("0.##");

        foreach (var tmpText in _heroSellPrice)
        {
            tmpText.text = $"SELL\n{QuantumConnection.GameConfig.GetHeroSellCost(heroInfo.Rare, level):0.##} coins";
        }
    }

    protected void UpdateBar(Slider slider, RectTransform rectTransform, FP amount, FP maxAmount, float progress = 0)
    {
        slider.gameObject.SetActive(true);
        slider.value = (amount / maxAmount).AsFloat;
        rectTransform.anchoredPosition = new Vector2(
            Mathf.Lerp(0, rectTransform.rect.width, progress),
            rectTransform.anchoredPosition.y
        );
    }
}
