using System.Collections.Generic;
using System.Linq;
using Luna.UI;
using Luna.UI.Navigation;
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
                }
            }
        }


        private void OnCellClickOrSubmit(int index, FixedListViewCell<YamanoteCategory> cell)
        {
            var categoryCell = (YamanoteCategoryListCell)cell;
            
            Navigator.Push<YamanoteQuestionsView>((view) =>
            {
                var category = Data.Find(c => c.Name == categoryCell.text.text);
                if (category != null)   
                    view.Category = category;
            });
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

        protected override void OnCellDeselected(int index, YamanoteCategoryListCell listViewCell)
        {
            listViewCell.text.color = Color.white;
        }

        protected override void OnCellSelected(int index, YamanoteCategoryListCell listViewCell)
        {
            listViewCell.text.color = Color.black;
        }
    }

}