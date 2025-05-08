using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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
            
            inputField.onValueChanged.AddListener(CheckText);
            // inputField.onSubmit.AddListener(async (value) => {
            //     await UniTask.NextFrame();
            //     saveAndPlayButton.Select();
            // });
            inputField.Select();
            
            CheckText(Question?.Content);
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
                inputField.DeactivateInputField();
                PopupConfirmView();
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
        
        private void CheckText(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                bottomPanel.blueButton.gameObject.SetActive(false);
                bottomPanel.redButton.gameObject.SetActive(false);
            }
            else
            {
                bottomPanel.blueButton.gameObject.SetActive(true);
                bottomPanel.redButton.gameObject.SetActive(true);
            }
        }
        
        private void PopupConfirmView()
        {
            Navigator.ShowModal<AlertDialogue>((dialogue) => {
                dialogue.Content = "編集中の内容を保存せずに終了しますか？";
                dialogue.onConfirm = () =>
                {
                    Navigator.Pop();
                    Navigator.Pop((question: Question, shouldPlay: false));
                };
                dialogue.onCancel = async () =>
                {
                    await UniTask.NextFrame();
                    inputField.Select();
                    inputField.ActivateInputField();
                };
            });
        }
    }
}
