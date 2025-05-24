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
            if (chooseNumber == Hero.UpgradeVariant1)
            {
                return "“Unmoving Rite” heals all allies in a radius for 150% of base damage, for 2 sec";
            }
            else if (chooseNumber == Hero.UpgradeVariant2)
            {
                return "“Unmoving Rite” gives allies in the radius +20% to attack speed for 4 sec";
            }
        }
        else if (level == Hero.Level3)
        {
            if (chooseNumber == Hero.UpgradeVariant1)
            {
                return "Increases the radius of the ritual by 1 cell";
            }
            else if (chooseNumber == Hero.UpgradeVariant2)
            {
                return "Reduces the ability's cooldown by 3 sec and increases its duration by 2 sec (for Taunt effects only)";
            }
        }

        return string.Empty;
    }
}
