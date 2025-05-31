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
        HeroNameEnum heroName = QuantumConnection.GetHeroInfo(id, out _).Name;

        return heroName switch
        {
            HeroNameEnum.TinMan => GetTinManDescription(level, chooseNumber),
            HeroNameEnum.Nutcracker => GetNutcrackerDescription(level, chooseNumber),
            HeroNameEnum.KingArthur => GetKingArthurDescription(level, chooseNumber),
            HeroNameEnum.Beast => GetBeastDescription(level, chooseNumber),
            HeroNameEnum.StoneGolem => GetStoneGolemDescription(level, chooseNumber),
            HeroNameEnum.BabaYaga => GetBabaYagaDescription(level, chooseNumber),
            _ => string.Empty,
        };
    }

    private string GetTinManDescription(int level, int chooseNumber)
    {
        if (level == Hero.Level2)
        {
            if (chooseNumber == Hero.UpgradeVariant1)
            {
                return "“Unmoving Rite”\nHeals all allies in a radius for 150% of base damage, for 2 sec";
            }
            else if (chooseNumber == Hero.UpgradeVariant2)
            {
                return "“Unmoving Rite”\nGives allies in the radius +20% to attack speed for 4 sec";
            }
        }
        else if (level == Hero.Level3)
        {
            if (chooseNumber == Hero.UpgradeVariant1)
            {
                return "“Unmoving Rite”\nIncreases the radius by 1 cell";
            }
            else if (chooseNumber == Hero.UpgradeVariant2)
            {
                return "“Unmoving Rite”\nReduces the cooldown by 3 sec and increases its duration by 2 sec (for Taunt effect only)";
            }
        }

        return string.Empty;
    }


    private string GetNutcrackerDescription(int level, int chooseNumber)
    {
        if (level == Hero.Level2)
        {
            if (chooseNumber == Hero.UpgradeVariant1)
            {
                return "“CLICK!”\nStuns the target for an additional 1 second if it has less than 50% mana after being incinerated";
            }
            else if (chooseNumber == Hero.UpgradeVariant2)
            {
                return "“CLICK!”\nAlso deals 50% damage to two adjacent enemies";
            }
        }
        else if (level == Hero.Level3)
        {
            if (chooseNumber == Hero.UpgradeVariant1)
            {
                return "On death, the Nutcracker automatically applies a “CLICK!” to the nearest enemy";
            }
            else if (chooseNumber == Hero.UpgradeVariant2)
            {
                return "Gains +100% to maximum health";
            }
        }

        return string.Empty;
    }

    private string GetKingArthurDescription(int level, int chooseNumber)
    {
        if (level == Hero.Level2)
        {
            if (chooseNumber == Hero.UpgradeVariant1)
            {
                return "“Battle Cry”\nnow also restores 25% of maximum mana to all allies";
            }
            else if (chooseNumber == Hero.UpgradeVariant2)
            {
                return "“Battle Cry”\nnow also restores 15% of maximum HP to all allies";
            }
        }
        else if (level == Hero.Level3)
        {
            if (chooseNumber == Hero.UpgradeVariant1)
            {
                return "“Battle Cry”\nIf Arthur's HP > 50% when activated, the buff lasts 10 sec";
            }
            else if (chooseNumber == Hero.UpgradeVariant2)
            {
                return "“Battle Cry”\nAfter the buff, Arthur is invulnerable for 3 sec.";
            }
        }

        return string.Empty;
    }

    private string GetBeastDescription(int level, int chooseNumber)
    {
        if (level == Hero.Level2)
        {
            if (chooseNumber == Hero.UpgradeVariant1)
            {
                return "“Tag”\nnow also reduces the mana accumulation of the character under the tag by 50%";
            }
            else if (chooseNumber == Hero.UpgradeVariant2)
            {
                return "“Tag”\nlasts 1 second longer";
            }
        }
        else if (level == Hero.Level3)
        {
            if (chooseNumber == Hero.UpgradeVariant1)
            {
                return "“Tag”\nnow also imposes a stun effect on the target";
            }
            else if (chooseNumber == Hero.UpgradeVariant2)
            {
                return "“Tag”\nlasts 1 second longer";
            }
        }

        return string.Empty;
    }

    private string GetStoneGolemDescription(int level, int chooseNumber)
    {
        if (level == Hero.Level2)
        {
            if (chooseNumber == Hero.UpgradeVariant1)
            {
                return "“TITAN'S Punch”\nEnemies that are hit get -15% to armor for 4 seconds";
            }
            else if (chooseNumber == Hero.UpgradeVariant2)
            {
                return "“TITAN'S Punch”\nslows down affected enemies by 30% for 2 seconds";
            }
        }
        else if (level == Hero.Level3)
        {
            if (chooseNumber == Hero.UpgradeVariant1)
            {
                return "15% to armor and HP";
            }
            else if (chooseNumber == Hero.UpgradeVariant2)
            {
                return "“TITAN'S Punch”\n+100% damage.";
            }
        }

        return string.Empty;
    }

    private string GetBabaYagaDescription(int level, int chooseNumber)
    {
        if (level == Hero.Level2)
        {
            if (chooseNumber == Hero.UpgradeVariant1)
            {
                return "“Poison Spit”\nDoT damage lasts for 6 seconds and does 120% damage";
            }
            else if (chooseNumber == Hero.UpgradeVariant2)
            {
                return "“Poison Spit”\nalso heals Baba Yaga for 100% of the ability's damage";
            }
        }
        else if (level == Hero.Level3)
        {
            if (chooseNumber == Hero.UpgradeVariant1)
            {
                return "Baba Yaga gets 3 coins for each kill";
            }
            else if (chooseNumber == Hero.UpgradeVariant2)
            {
                return "Baba Yaga gains +5 mana/sec regeneration and reduces the cooldown on “Poison Spit” by 1 second";
            }
        }

        return string.Empty;
    }
}
