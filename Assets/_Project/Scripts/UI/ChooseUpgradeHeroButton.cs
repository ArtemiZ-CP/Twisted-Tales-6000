using Quantum.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChooseUpgradeHeroButton : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _description;

    public event System.Action OnClick;

    private void OnEnable()
    {
        _button.onClick.AddListener(OnButtonClick);
    }

    private void OnDisable()
    {
        _button.onClick.RemoveListener(OnButtonClick);
    }

    public void Initialize(int id, int level, int chooseNumber)
    {
        _description.text = GetDescription(id, level, chooseNumber);
    }

    private void OnButtonClick()
    {
        OnClick?.Invoke();
    }

    private string GetDescription(int id, int level, int chooseNumber)
    {
        HeroNameEnum heroName = QuantumConnection.GetHeroInfo(id).Name;

        if (heroName == HeroNameEnum.TinMan)
        {
            return GetTinManDescription(level, chooseNumber);
        }

        return string.Empty;
    }

    private string GetTinManDescription(int level, int chooseNumber)
    {
        if (level == Hero.Level2)
        {
            if (chooseNumber == Hero.UpgradeLevel1)
            {
                return "Upgrade to Level 3";
            }
            else if (chooseNumber == Hero.UpgradeLevel2)
            {
                return "Upgrade to Level 4";
            }
        }
        else if (level == Hero.Level3)
        {
            if (chooseNumber == Hero.UpgradeLevel1)
            {
                return "Upgrade to Level 4";
            }
            else if (chooseNumber == Hero.UpgradeLevel2)
            {
                return "Upgrade to Level 5";
            }
        }

        return string.Empty;
    }
}
