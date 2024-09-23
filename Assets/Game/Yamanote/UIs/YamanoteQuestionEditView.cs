using Luna.UI;
using Luna.UI.Navigation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using USEN.Games.Common;

namespace USEN.Games.Yamanote
{
    public class YamanoteQuestionEditView : Widget
    {
        public TextMeshProUGUI titleText;
        public TMP_InputField inputField;
        public Button saveAndPlayButton;
        public Button saveButton;
        public BottomPanel bottomPanel;

        private void Start()
        {
            saveAndPlayButton.onClick.AddListener(OnSaveAndPlayButtonClicked);
            saveButton.onClick.AddListener(OnSaveButtonClicked);
        }

        private void OnSaveAndPlayButtonClicked()
        {
            Debug.Log("OnSaveAndPlayButtonClicked");
        }
    
        private void OnSaveButtonClicked()
        {
            Debug.Log("OnSaveButtonClicked");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) ||
                Input.GetButtonDown("Cancel")) {
                Navigator.Pop();
            }
        }
    }
}
