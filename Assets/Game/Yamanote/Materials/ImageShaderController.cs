// Created by LunarEclipse on 2024-7-29 14:38.

using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Games.Yamanote
{
    [ExecuteAlways]
    [RequireComponent(typeof(Image))]
    public class ImageShaderController : MonoBehaviour
    {
        [SerializeField, OnChanged(nameof(OnOffsetChange))]
        private Vector2 _offset;
        
        [SerializeField, OnChanged(nameof(OnTilingChange))]
        private Vector2 _tiling;
        
        [FormerlySerializedAs("_speed")] public Vector2 speed;
        
        private Image _image;
        private Material _material;
        
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");
        
        public Vector2 Offset
        {
            get => _offset;
            set
            {
                _offset = value;
                OnOffsetChange();
            }
        }
        
        public Vector2 Tiling
        {
            get => _tiling;
            set
            {
                _tiling = value;
                OnTilingChange();
            }
        }

        private void Awake()
        {
            _image = GetComponent<Image>();
            _material = _image.material;
        }
        
        private void Update()
        {
            if (speed != Vector2.zero)
            {
                _offset += speed * Time.deltaTime;
                OnOffsetChange();
            }
        }
        
        public void OnOffsetChange()
        {
            _material.SetTextureOffset(MainTex, _offset);
        }
        
        public void OnTilingChange()
        {
            _material.SetTextureScale(MainTex, _tiling);
        }
    }
}