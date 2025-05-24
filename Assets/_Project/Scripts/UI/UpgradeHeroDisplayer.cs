using Quantum;
using Quantum.Game;
using UnityEngine;

public class UpgradeHeroDisplayer : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private HeroesMover _heroesMover;
    [SerializeField] private ChooseUpgradeHeroButton _firstUpgradeButton;
    [SerializeField] private ChooseUpgradeHeroButton _secondUpgradeButton;

    private int _id = -1;
    private int _level = -1;

    private void Awake()
    {
        _panel.SetActive(false);
    }

    private void OnEnable()
    {
        _heroesMover.OnUpgradeHero += ShowPanel;
        _firstUpgradeButton.OnClick += SelectFirstUpgrade;
        _secondUpgradeButton.OnClick += SelectSecondUpgrade;
    }

    private void OnDisable()
    {
        _heroesMover.OnUpgradeHero -= ShowPanel;
        _firstUpgradeButton.OnClick -= SelectFirstUpgrade;
        _secondUpgradeButton.OnClick -= SelectSecondUpgrade;
    }

    private void ShowPanel(int id, int level)
    {
        _id = id;
        _level = level;
        _firstUpgradeButton.Initialize(id, level, Hero.UpgradeVariant1);
        _secondUpgradeButton.Initialize(id, level, Hero.UpgradeVariant2);
        _panel.SetActive(true);
    }

    private void SelectFirstUpgrade()
    {
        Select(Hero.UpgradeVariant1);
        HidePanel();
    }

    private void SelectSecondUpgrade()
    {
        Select(Hero.UpgradeVariant2);
        HidePanel();
    }

    private void HidePanel()
    {
        _panel.SetActive(false);
    }

    private void Select(int index)
    {
        if (QuantumConnection.IsAbleToConnectQuantum())
        {
            CommandUpgradeHero commandMoveHero = new()
            {
                HeroID = _id,
                HeroLevel =  _level,
                UpgradeLevel = index
            };

            QuantumRunner.DefaultGame.SendCommand(commandMoveHero);
        }
    }
}
