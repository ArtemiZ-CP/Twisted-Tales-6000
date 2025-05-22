using Quantum;
using Quantum.Game;
using UnityEngine;

public class UpgradeHeroDisplayer : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private HeroesMover _heroesMover;
    [SerializeField] private UnityEngine.UI.Button _firstUpgradeButton;
    [SerializeField] private UnityEngine.UI.Button _secondUpgradeButton;

    private int _id = -1;
    private int _level = -1;

    private void Awake()
    {
        _panel.SetActive(false);
    }

    private void OnEnable()
    {
        _heroesMover.OnUpgradeHero += ShowPanel;
        _firstUpgradeButton.onClick.AddListener(SelectFirstUpgrade);
        _secondUpgradeButton.onClick.AddListener(SelectSecondUpgrade);
    }

    private void OnDisable()
    {
        _heroesMover.OnUpgradeHero -= ShowPanel;
        _firstUpgradeButton.onClick.RemoveListener(SelectFirstUpgrade);
        _secondUpgradeButton.onClick.RemoveListener(SelectSecondUpgrade);
    }

    private void ShowPanel(int id, int level)
    {
        _id = id;
        _level = level;
        _panel.SetActive(true);
    }

    private void SelectFirstUpgrade()
    {
        Select(Hero.UpgradeLevel1);
        HidePanel();
    }

    private void SelectSecondUpgrade()
    {
        Select(Hero.UpgradeLevel2);
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
