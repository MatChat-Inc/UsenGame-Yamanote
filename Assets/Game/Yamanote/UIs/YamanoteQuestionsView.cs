using Luna.UI;
using Luna.UI.Navigation;
using TMPro;
using UnityEngine;
using USEN.Games.Common;

namespace USEN.Games.Yamanote
{
    public class YamanoteQuestionsView : Widget
    {
        public TextMeshProUGUI titleText;
        public YamanoteQuestionsList listView;
        public BottomPanel bottomPanel;
    
        private YamanoteCategory _category;
        public YamanoteCategory Category
        {
            get => _category;
            set
            {
                _category = value;
                titleText.text = value.Name;
                listView.Data = value.Questions;
            }
        }
    
        // public YamanoteDataset dataset;
    
        void OnEnable()
        {
            listView.FocusOnCell(0);
        }
    
        void Start()
        {
        
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
