using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Luna.UI;
using Luna.UI.Navigation;
using Modules.UI.Misc;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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

        private YamanoteQuestion _question;
        private List<YamanoteQuestion> _questions;

        public YamanoteQuestion Question
        {
            get => _question;
            set
            {
                _question = value;
                inputField.text = value.Content;
            }
        }
        
        private void Start()
        {
            // saveAndPlayButton.onClick.AddListener(OnSaveAndPlayButtonClicked);
            // saveButton.onClick.AddListener(OnSaveButtonClicked);
            bottomPanel.onBlueButtonClicked += OnSaveAndPlayButtonClicked;
            bottomPanel.onRedButtonClicked += OnSaveButtonClicked;
            if (!string.IsNullOrEmpty(Question?.Content)) {
                inputField.onFocusSelectAll = true;
            }
            // inputField.onSubmit.AddListener(async (value) => {
            //     await UniTask.NextFrame();
            //     saveAndPlayButton.Select();
            // });
            inputField.Select();
        }

        private void OnSaveAndPlayButtonClicked()
        {
            SaveChanges();
            Navigator.Pop((question: Question, shouldPlay: true));
        }
    
        private void OnSaveButtonClicked()
        {
            SaveChanges();
            Navigator.Pop((question: Question, shouldPlay: false));
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) ||
                Input.GetButtonDown("Cancel")) {
                Navigator.Pop((question: Question, shouldPlay: false));
            }
            
            // if (Input.GetKeyDown(KeyCode.DownArrow))
            // {
            //     if (EventSystem.current.currentSelectedGameObject == inputField.gameObject) 
            //         saveAndPlayButton.Select();
            // }
            // if (Input.GetKeyDown(KeyCode.UpArrow))
            // {
            //     if (EventSystem.current.currentSelectedGameObject == saveButton.gameObject ||
            //         EventSystem.current.currentSelectedGameObject == saveAndPlayButton.gameObject) {
            //         inputField.Select();
            //     }
            //         
            // }
            // if (Input.GetKeyDown(KeyCode.RightArrow) || 
            //     Input.GetKeyDown(KeyCode.LeftArrow))
            // {
            //     if (EventSystem.current.currentSelectedGameObject == saveButton.gameObject) 
            //         saveAndPlayButton.Select();
            //     else if (EventSystem.current.currentSelectedGameObject == saveAndPlayButton.gameObject) 
            //         saveButton.Select();
            // }
        }
        
        private void SaveChanges()
        {
            if (_question == null)
            {
                var newQuestion = new YamanoteQuestion() {
                    Content = inputField.text,
                    Category = "オリジナル",
                    Theme = "オリジナル",
                };
                _question = newQuestion;
                YamanoteDAO.Instance.AddQuestion(newQuestion);
            } else {
                _question.Content = inputField.text;
                YamanoteDAO.Instance.UpdateQuestion(_question);
            }
        }
    }
}
