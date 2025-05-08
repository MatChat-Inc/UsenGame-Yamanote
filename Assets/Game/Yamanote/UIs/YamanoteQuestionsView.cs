using System.Linq;
using Luna.UI;
using Luna.UI.Navigation;
using Modules.UI.Misc;
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

                if (Category.Name != "オリジナル")
                {
                    bottomPanel.redButton.gameObject.SetActive(false);
                    bottomPanel.blueButton.gameObject.SetActive(false);
                    bottomPanel.yellowButton.gameObject.SetActive(false);
                }
            }
        }
    
        // public YamanoteDataset dataset;
    
        void OnEnable()
        {
            // listView.FocusOnCell(0);
            
            bottomPanel.onBlueButtonClicked += OnBlueButtonClicked;
            bottomPanel.onRedButtonClicked += OnRedButtonClicked;
            bottomPanel.onYellowButtonClicked += OnYellowButtonClicked;
        }

        void OnDisable()
        {
            bottomPanel.onBlueButtonClicked -= OnBlueButtonClicked;
            bottomPanel.onRedButtonClicked -= OnRedButtonClicked;
            bottomPanel.onYellowButtonClicked -= OnYellowButtonClicked;
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

        private async void OnBlueButtonClicked()
        {
            var selectedQuestion = listView.SelectedData;
            
            var result = await Navigator.Push<YamanoteQuestionEditView>((view) => {
                view.Question = selectedQuestion;
            }) as (YamanoteQuestion question, bool shouldPlay)?;

            if (result == null) return;
                
            YamanoteDAO.Instance.UpdateQuestion(result.Value.question);
                
            if (result.Value.shouldPlay)
                PlayWithQuestion(result.Value.question);
            else
            {
                await listView.ReloadAsync();
                listView.FocusOnCell(Category.Questions.IndexOf(selectedQuestion));
            }
        }
                
        private async void OnRedButtonClicked()
        {
            var result = await Navigator.Push<YamanoteQuestionEditView>((view) => {
                // view.Question = newQuestion;
            }) as (YamanoteQuestion question, bool shouldPlay)?;
            
            if (result == null || string.IsNullOrEmpty(result.Value.question?.Content)) return;

            var question = result.Value.question;
            Category.Questions.Add(question);
            
            if (result.Value.shouldPlay)
                PlayWithQuestion(question);
            else {
                await listView.ReloadAsync();
                listView.FocusOnCell(listView.Count - 1);
            }
        }
        
        // Delete question
        private void OnYellowButtonClicked()
        {
            if (listView.SelectedData == null) return;
            PopupDeleteConfirmView();
        }
        
        public void PlaySequentially()
        {
            Navigator.Push<YamanoteGameView>((view) => {
                view.Questions = Category.Questions;
            });
        }
        
        public void PlaySelectedQuestion()
        {
            var selectedQuestion = listView.SelectedData;
            PlayWithQuestion(selectedQuestion);
        }

        public void PlayRandomQuestion()
        {
            Navigator.Push<YamanoteGameView>((view) => {
                view.Questions = Category.Questions.Shuffle().ToList();
            });
        }
        
        public void PlayWithQuestion(YamanoteQuestion question)
        {
            Navigator.Push<YamanoteGameView>((view) => {
                var questions = Category.Questions.Shuffle().ToList();
                
                // Move the selected question to the first position
                if (questions.Contains(question))
                    questions.Remove(question);
                questions.Insert(0, question);
                
                view.Questions = questions;
            });
        }

        private void DeleteSelectedQuestion()
        {
            var selectedQuestion = listView.SelectedData;
            listView.RemoveSelected();
            YamanoteDAO.Instance.DeleteQuestion(selectedQuestion);
        }
        
        private void PopupDeleteConfirmView()
        {
            Navigator.ShowModal<AlertDialogue>((dialogue) =>
            {
                dialogue.Title = "削除しますか？";
                dialogue.Content = "※一度削除した問題は復元できません。";
                dialogue.onConfirm = () =>
                {
                    Navigator.Pop();
                    DeleteSelectedQuestion();
                };
            });
        }
    }
}
