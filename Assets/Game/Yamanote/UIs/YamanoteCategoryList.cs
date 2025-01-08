using System.Collections.Generic;
using System.Linq;
using Luna;
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
        
        protected void Start()
        {
            var content = _scrollRect.content;
            var children = content.GetComponentsInChildren<YamanoteCategoryListCell>();

            if (children.Length > 0)
            {
                var firstCell = children[0];
                firstCell.Focus();
                
                foreach (var cell in children)
                {
                    cell.OnCellClicked += OnCellClickOrSubmit;
                    cell.OnCellSubmitted += OnCellClickOrSubmit;
                    cell.OnCellSelected += OnCellSelected;
                }
            }
            
            _initialized = true;
        }


        private void OnCellClickOrSubmit(int index, FixedListViewCell<YamanoteCategory> cell)
        {
            var categoryCell = (YamanoteCategoryListCell)cell;
            var category = Data.Find(c => c.Name == categoryCell.text.text) ?? new YamanoteCategory() {
                Name = categoryCell.text.text,
            };

            if (YamanotePreferences.DisplayMode == YamanoteDisplayMode.Random)
            {
                // Play random game
                if (category != null)
                {
                    var questions = category.Questions.Shuffle().ToList();
                    Navigator.Push<YamanoteGameView>((view) => {
                        view.Questions = questions;
                    });
                }
            }
            else Navigator.Push<YamanoteQuestionsView>((view) => {
                if (category != null)   
                    view.Category = category;
            });
            
            SFXManager.Play(R.Audios.SfxConfirm);
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
                var angle = Mathf.Asin((cellTransform.anchoredPosition.y + RectTransform.rect.height * 0.5f - RingCenter.y + _scrollRect.content.anchoredPosition.y) / RingRadius);
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
    }

}
