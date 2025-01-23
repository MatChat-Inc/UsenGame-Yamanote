using Cysharp.Threading.Tasks;
using Luna.Extensions.Unity;
using Luna.UI.Navigation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace USEN.Games.Yamanote
{
    [RequireComponent(typeof(CanvasGroup))]
    public class YamanoteCategoryListCell : FixedListViewCell<YamanoteCategory>
    {
        public TextMeshProUGUI text;
        public Image background;
        public CircleCollider2D ringCollider;
        
        [HideInInspector]
        public CanvasGroup canvasGroup;

        public override YamanoteCategory Data { get; set; }

        protected override void Awake()
        {
            base.Awake();
            
            canvasGroup = GetComponent<CanvasGroup>();
        }

        protected override void Start()
        {
            OnCellSelected += OnSelected;
            OnCellDeselected += OnDeselected;
        }
        
        private void OnSelected(int arg1, FixedListViewCell<YamanoteCategory> arg2)
        {
            text.color = Color.HSVToRGB(148f / 360, 0.9f, 0.6f);
        }
    
        private void OnDeselected(int arg1, FixedListViewCell<YamanoteCategory> arg2)
        {
            text.color = Color.black;
        }

        protected async void OnValidate()
        {
            await UniTask.DelayFrame(5);
        
            if (ringCollider != null)
            {
                // Cast a horizontal ray to ring collider to get collision point
                var ray = new Ray( new Vector3(ringCollider.transform.position.x + 1000f, background.transform.position.y, 0), Vector3.left);
                var hit = Physics2D.Raycast(ray.origin, ray.direction, 1000f, LayerMask.GetMask("Editor"));
                // Debug.Log(hit.point);
            
                if (hit.collider != null)
                {
                    background.transform.SetX(hit.point.x);
                }
            }
        }
    
    
    }
}


