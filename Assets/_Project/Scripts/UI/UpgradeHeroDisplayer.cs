using Quantum.Game;
using UnityEngine;

public class UpgradeHeroDisplayer : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private HeroesMover _heroesMover;

    private void Awake()
    {
        _panel.SetActive(false);
    }

    private void OnEnable()
    {
        _heroesMover.OnUpgradeHero += ShowPanel;
    }

    private void OnDisable()
    {
        _heroesMover.OnUpgradeHero -= ShowPanel;
    }

    public void ShowPanel()
    {
        _panel.SetActive(true);
    }

    public void HidePanel()
    {
        _panel.SetActive(false);
    }
}
