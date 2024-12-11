using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Luna.Extensions;
using Luna.Extensions.Unity;
using Luna.UI;
using UnityEngine;
using UnityEngine.UI;

namespace USEN.Games.Yamanote
{
    public class YamanoteQuestionsPicker: ListView<YamanoteQuestionsPickerCell, string>
    {
        protected override void OnCellScrolling(int index, YamanoteQuestionsPickerCell cell, float delta)
        {
            // Change cell's alpha based on distance from center
            var height = cell.RectTransform.rect.height;
            var distance = Mathf.Abs(delta.Mod(height) - height / 2);
            cell.text.color = cell.text.color.WithAlpha(distance * 3 / height - 0.5f);
            
            //Debug.Log($"Cell {index} is scrolling with delta {delta} and distance {distance}");
        }

        public async Task ScrollTo(int index, float duration = 0.5f)
        {
            var cellHeight = cellPrefab.RectTransform.rect.height;
            var y = index * cellHeight;
            var targetPosition = new Vector2(0, y);
            DOTween.To(() => _scrollRect.content.anchoredPosition, v => _scrollRect.content.anchoredPosition = v, targetPosition, duration);
            await UniTask.Delay((int) (duration * 1000));
        }
        
        public async Task PickNextQuestion()
        {
            await ScrollTo(FirstVisibleIndex + 1, 2);
        }
    }
}