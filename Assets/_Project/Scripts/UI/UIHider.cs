using UnityEngine;

public class UIHider : MonoBehaviour
{
    [SerializeField] private GameObject[] _uiElements;

    private bool _isUIHidden = false;

    private void Update()
    {
        if (PlayerInput.SwitchUI())
        {
            _isUIHidden = !_isUIHidden;

            foreach (var element in _uiElements)
            {
                element.SetActive(_isUIHidden);
            }
        }
    }
}
