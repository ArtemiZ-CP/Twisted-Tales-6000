using UnityEngine;

public class UpgradeButton : MonoBehaviour
{
    private int _id = -1;
    private int _level = 0;

    public int ID => _id;
    public int Level => _level;

    public void ShowButton(int heroID, int heroLevel)
    {
        gameObject.SetActive(true);
        _id = heroID;
        _level = heroLevel;
    }

    public void HideButton()
    {
        gameObject.SetActive(false);
    }
}