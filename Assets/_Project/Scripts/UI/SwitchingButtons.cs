namespace Quantum.Game
{
    using UnityEngine;
    using UnityEngine.UI;
    public class SwitchingButtons : MonoBehaviour
    {
        [SerializeField] private ButtonInfo[] _buttons;

        private void Start()
        {
            foreach (var button in _buttons)
            {
                button.Button.onClick.AddListener(() => Switch(button));
            }

            Switch(_buttons[0]);
        }

        private void Switch(ButtonInfo buttonInfo)
        {
            buttonInfo.Panel.SetActive(true);
            buttonInfo.Image.color = Color.white;

            foreach (var button in _buttons)
            {
                if (button.Button != buttonInfo.Button)
                {
                    button.Panel.SetActive(false);
                    button.Image.color = Color.gray;
                }
            }
        }

        [System.Serializable]
        private struct ButtonInfo
        {
            public Button Button;
            public GameObject Panel;

            private Image _image;

            public Image Image => _image ??= Button.GetComponent<Image>();
        }
    }
}