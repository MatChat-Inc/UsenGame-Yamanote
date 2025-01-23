using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Luna;
using Luna.UI;
using Luna.UI.Navigation;
using Modules.UI.Misc;
using UnityEngine;
using UnityEngine.EventSystems;

namespace USEN.Games.Yamanote
{
    
    public class YamanoteQuestionsList : FixedListView<YamanoteQuestionsListCell, YamanoteQuestion>, IEventSystemHandler
    {
        public RectTransform ring;

        private Vector2 RingCenter => ring.anchoredPosition - RectTransform.anchoredPosition;
        private float RingRadius => ring.rect.width / 2;
        
        private bool _isNavigating = false;
        
        private RectTransform RectTransform => (RectTransform)transform;


        protected override void Start()
        {
            base.Start();

            FadeIn();
        }

        private void Update()
        {
            // Pin every cell to the circumference of the ring
            UpdateCellPositions();
            Debug.Log($"IndexFromEdge: {VisibleIndex}");
        }

        private void UpdateCellPositions()
        {
            for (int i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];
                var cellTransform = (RectTransform)cell.transform;
                var y = cellTransform.anchoredPosition.y;
                var angle = Mathf.Asin((cellTransform.anchoredPosition.y + RectTransform.rect.height * 0.5f - RingCenter.y + _scrollRect.content.anchoredPosition.y) / RingRadius);
                var x = RingRadius * Mathf.Cos(angle) - 50f;
                var position = new Vector2(x, y);
                cellTransform.anchoredPosition = position;
            }
        }

        private async void OnCellClickOrSubmit(int index, YamanoteQuestionsListCell cell)
        {
            if (_isNavigating) return;
            _isNavigating = true;
            
            SFXManager.Play(R.Audios.SfxConfirm);
            
            Navigator.Push<YamanoteGameView>((view) => {
                var questions = Data.Shuffle().ToList();
                
                // Move the selected question to the first position
                var selectedQuestion = Data[index];
                questions.Remove(selectedQuestion);
                questions.Insert(0, selectedQuestion);
                
                view.Questions = questions;
            });
            
            _isNavigating = false;
        }

        protected override void OnCellClicked(int index, YamanoteQuestionsListCell listViewCell)
        {
            OnCellClickOrSubmit(index, listViewCell);
        }
        
        protected override void OnCellSubmitted(int index, YamanoteQuestionsListCell listViewCell)
        {
            OnCellClickOrSubmit(index, listViewCell);
        }

        protected override void OnCellDeselected(int index, YamanoteQuestionsListCell listViewCell)
        {
            listViewCell.text.color = Color.black;
        }

        protected override void OnCellSelected(int index, YamanoteQuestionsListCell listViewCell)
        {
            listViewCell.text.color = Color.HSVToRGB(148f / 360, 0.9f, 0.6f);
            
            if (Initialized)
                SFXManager.Play(R.Audios.SfxSelect);
        }
        
        public Task FadeIn(float duration = 0.5f, float delay = 0.05f)
        {
            for (var index = 0; index < cells.Count; index++)
            {
                var cell = cells[index];
                
                // Animate cells
                var currentPos = cell.background.rectTransform.anchoredPosition;
                cell.background.rectTransform.anchoredPosition = new Vector2(-960, currentPos.y);
                cell.canvasGroup.alpha = 0;
                cell.background.rectTransform.DOAnchorPosX(currentPos.x, duration).SetDelay(index * delay);
                cell.canvasGroup.DOFade(1, duration).SetDelay(index * delay);
            }
            
            return Task.Delay((int)((duration + delay * 5) * 1000));
        }
        
        public Task FadeOut(float duration = 0.5f, float delay = 0.05f)
        {
            var startIndex = Mathf.Min(SelectedIndex - VisibleIndex, cells.Count - 5);
            for (var i = 0; i < Mathf.Min(cells.Count, startIndex + 5); i++)
            {
                var cell = cells[startIndex + i];
                
                // Animate cells
                cell.canvasGroup.alpha = 1;
                cell.background.rectTransform.DOAnchorPosX(-960, duration).SetDelay(i * delay);
                cell.canvasGroup.DOFade(0, duration).SetDelay(i * delay);
            }
            
            return Task.Delay((int)((duration + delay * 5) * 1000));
        }
        
        public void ResetCellBackgrounds()
        {
            foreach (var cell in cells)
            {
                cell.background.rectTransform.anchoredPosition = new Vector2(250, 0);
                cell.canvasGroup.alpha = 1;
            }
        }
    }

}
