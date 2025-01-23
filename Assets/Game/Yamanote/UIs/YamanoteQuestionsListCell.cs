// Created by LunarEclipse on 2024-7-29 20:59.

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace USEN.Games.Yamanote
{
    [RequireComponent(typeof(CanvasGroup))]
    public class YamanoteQuestionsListCell : FixedListViewCell<YamanoteQuestion>
    {
        public TextMeshProUGUI text;
        public Image background;
        public CircleCollider2D ringCollider;

        [HideInInspector]
        public CanvasGroup canvasGroup;

        public override YamanoteQuestion Data
        {
            get => data;
            set
            {
                data = value;
                text.text = value.Content;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            
            canvasGroup = GetComponent<CanvasGroup>();
        }
    }
}