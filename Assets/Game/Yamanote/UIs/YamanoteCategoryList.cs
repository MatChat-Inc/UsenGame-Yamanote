using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Luna;
using Luna.Extensions.Unity;
using Luna.UI;
using Luna.UI.Navigation;
using Modules.UI.Misc;
using UnityEngine;
using UnityEngine.EventSystems;

namespace USEN.Games.Yamanote
{
    public class YamanoteCategoryList : FixedListView<YamanoteCategoryListCell, YamanoteCategory>, IEventSystemHandler
    {
        public RectTransform ring;

        private Vector2 RingCenter => ring.anchoredPosition - RectTransform.anchoredPosition;
        private float RingRadius => ring.rect.width / 2;
        
        private RectTransform RectTransform => (RectTransform)transform;

        private bool _initialized;
        private bool _isNavigating;
        public new List<YamanoteCategoryListCell> cells;
        
        protected void Start()
        {
            var content = _scrollRect.content;
            var children = content.GetComponentsInChildren<YamanoteCategoryListCell>();
            cells = children.ToList();
            
            if (children.Length > 0)
            {
                var firstCell = children[0];
                firstCell.Focus();

                for (var index = 0; index < children.Length; index++)
                {
                    var cell = children[index];
                    cell.OnCellClicked += OnCellClickOrSubmit;
                    cell.OnCellSubmitted += OnCellClickOrSubmit;
                    cell.OnCellSelected += OnCellSelected;

                    // Animate cells
                    // var currentPos = cell.background.rectTransform.anchoredPosition;
                    cell.background.rectTransform.anchoredPosition = new Vector2(-960, 0);
                    cell.canvasGroup.alpha = 0;
                    cell.background.rectTransform.DOAnchorPosX(250, 0.5f).SetDelay(index * 0.05f);
                    cell.canvasGroup.DOFade(1, 0.5f).SetDelay(index * 0.05f);
                }
            }
            
            _initialized = true;
        }

        private async void OnCellClickOrSubmit(int index, FixedListViewCell<YamanoteCategory> cell)
        {
            if (_isNavigating) return;
            _isNavigating = true;
            
            var categoryCell = (YamanoteCategoryListCell)cell;
            var category = Data.Find(c => c.Name == categoryCell.text.text) ?? new YamanoteCategory() {
                Name = categoryCell.text.text,
            };
            
            SFXManager.Play(R.Audios.SfxConfirm);
            
            await FadeOut(0.4f, 0.04f);
            
            if (this == null)
                return;
            
            if (YamanotePreferences.DisplayMode == YamanoteDisplayMode.Random)
            {
                // Play random game
                if (category != null)
                {
                    var questions = category.Questions.Shuffle().ToList();
                    Navigator.Push<YamanoteGameView>((view) => {
                        view.Questions = questions;
                        ResetCellBackgrounds();
                    });
                }
            }
            else Navigator.Push<YamanoteQuestionsView>((view) =>
            {
                view.Category = category;
                ResetCellBackgrounds();
            });
            
            _isNavigating = false;
        }


        private void Update()
        {
            // Pin every cell to the circumference of the ring
            UpdateCellPositions();
        }

        private void UpdateCellPositions()
        {
            for (int i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];
                var cellTransform = (RectTransform)cell.transform;
                var y = cellTransform.anchoredPosition.y;
                var angle = Mathf.Asin((cellTransform.anchoredPosition.y + RectTransform.rect.height * 0.5f - RingCenter.y + _scrollRect.content.anchoredPosition.y - 30) / RingRadius);
                var x = RingRadius * Mathf.Cos(angle) - 50f;
                var position = new Vector2(x, y);
                cellTransform.anchoredPosition = position;
            }
        }

        private void OnCellSelected(int index, FixedListViewCell<YamanoteCategory> listViewCell)
        {
            if (_initialized)
                SFXManager.Play(R.Audios.SfxSelect);
        }
        
        public Task FadeIn(float duration = 0.5f, float delay = 0.05f)
        {
            var content = _scrollRect.content;
            var children = content.GetComponentsInChildren<YamanoteCategoryListCell>();
            for (var index = 0; index < children.Length; index++)
            {
                var cell = children[index];
                
                // Animate cells
                var currentPos = cell.background.rectTransform.anchoredPosition;
                cell.background.rectTransform.anchoredPosition = new Vector2(-960, currentPos.y);
                cell.canvasGroup.alpha = 0;
                cell.background.rectTransform.DOAnchorPosX(currentPos.x, duration).SetDelay(index * delay).SetAutoKill();
                cell.canvasGroup.DOFade(1, duration).SetDelay(index * delay);
            }
            
            return Task.Delay((int)((duration + delay * cells.Count) * 1000));
        }
        
        public Task FadeOut(float duration = 0.5f, float delay = 0.05f)
        {
            var content = _scrollRect.content;
            var children = content.GetComponentsInChildren<YamanoteCategoryListCell>();
            for (var index = 0; index < children.Length; index++)
            {
                var cell = children[index];
                
                // Animate cells
                cell.canvasGroup.alpha = 1;
                cell.background.rectTransform.DOAnchorPosX(-960, duration).SetDelay(index * delay);
                cell.canvasGroup.DOFade(0, duration).SetDelay(index * delay);
            }
            
            return Task.Delay((int)((duration + delay * cells.Count) * 1000));
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
