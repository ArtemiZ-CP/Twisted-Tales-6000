using UnityEngine;

public class EventSystemToggle : MonoBehaviour
{
    [SerializeField] private GameObject _eventSystem;

    private void Awake()
    {
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>(FindObjectsInactive.Exclude) == null)
        {
            _eventSystem.SetActive(true);
        }
    }
}
