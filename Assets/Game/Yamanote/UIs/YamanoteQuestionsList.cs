using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using Luna.Extensions.Unity;
using Luna.UI;
using Luna.UI.Navigation;
using UnityEngine;
using UnityEngine.EventSystems;

namespace USEN.Games.Yamanote
{
    
    public class YamanoteQuestionsList : FixedListView<YamanoteQuestionsListCell, YamanoteQuestion>, IEventSystemHandler
    {
        public RectTransform ring;

        private Vector2 RingCenter => ring.anchoredPosition - RectTransform.anchoredPosition;
        private float RingRadius => ring.rect.width / 2;
        
        private RectTransform RectTransform => (RectTransform)transform;

        
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

        protected override void OnCellClicked(int index, YamanoteQuestionsListCell listViewCell)
        {
            Navigator.Push<YamanoteGameView>((view) =>
            {
                view.Questions = Data;
            });
        }
        
        protected override void OnCellSubmitted(int index, YamanoteQuestionsListCell listViewCell)
        {
            Navigator.Push<YamanoteGameView>((view) =>
            {
                view.Questions = Data;
            });
        }

        protected override void OnCellDeselected(int index, YamanoteQuestionsListCell listViewCell)
        {
            listViewCell.text.color = Color.black;
        }

        protected override void OnCellSelected(int index, YamanoteQuestionsListCell listViewCell)
        {
            listViewCell.text.color = Color.HSVToRGB(148f / 360, 0.9f, 0.6f);
        }
    }

}